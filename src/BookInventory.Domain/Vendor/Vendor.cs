using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Vendor;

/// <summary>
/// One vendor or publication of one company (spec vendor R1-R14). A company-owned aggregate root (platform P18): its company is
/// stamped and filtered by the platform. It owns the master data (R2, R3), the bank details (R5), the opening balance (R6) and
/// the <c>ACTIVE</c>/<c>INACTIVE</c> status (R7); every rule of those is enforced here, and nothing outside changes its
/// properties. The code is generated once at creation from the company code series (platform P23) and never changes (R11).
/// </summary>
public class Vendor : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int CodeMaxLength = 20;
    public const int BankTextMaxLength = 150;

    /// <summary>R11: the prefix of the vendor codes in the company code series (<c>VEN-0001</c>).</summary>
    public const string CodePrefix = "VEN";

    private Vendor()
    {
        Code = string.Empty;
        Name = string.Empty;
        NameNormalized = string.Empty;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    /// <summary>R4: the name in the form two names are compared in (single spaces, lower case).</summary>
    public string NameNormalized { get; private set; }

    public string? Address { get; private set; }

    public string? ContactPerson { get; private set; }

    public string? Mobile { get; private set; }

    public string? Email { get; private set; }

    public string? Gstin { get; private set; }

    public string? State { get; private set; }

    public string? BankAccountHolder { get; private set; }

    public string? BankName { get; private set; }

    public string? BankBranch { get; private set; }

    /// <summary>R5, R12: sensitive; never written to the audit trail and masked in every list.</summary>
    [AuditSecret]
    public string? BankAccountNumber { get; private set; }

    public string? BankIfsc { get; private set; }

    public decimal OpeningBalanceAmount { get; private set; }

    public VendorStatus Status { get; private set; }

    public bool IsActive => Status == VendorStatus.Active;

    /// <summary>R5: the bank values as one block, or none.</summary>
    public BankAccount? Bank => BankAccountNumber is null
        ? null
        : new BankAccount(BankAccountHolder, BankName, BankBranch, BankAccountNumber, BankIfsc);

    /// <summary>R4: the form a name is compared in; the store uses it to find a vendor with the same name.</summary>
    public static string NormalizeName(string? name) => VendorRules.NormalizeName(name);

    /// <summary>R2-R7, R11: a new vendor, active, with the generated code.</summary>
    public static Vendor Create(string code, VendorDetails details)
    {
        var vendor = new Vendor { Code = code, Status = VendorStatus.Active };
        vendor.Apply(details);
        vendor.AddDomainEvent(new VendorCreated(vendor.Id, vendor.Code, vendor.Name));
        return vendor;
    }

    /// <summary>R2-R6: changes the master data; the code and the status never change here.</summary>
    public void Edit(VendorDetails details) => Apply(details);

    /// <summary>R7: switches the vendor off; its data and history stay.</summary>
    public void Deactivate()
    {
        if (Status == VendorStatus.Inactive)
            throw new ConflictException(VendorMessages.AlreadyInactive);

        Status = VendorStatus.Inactive;
        AddDomainEvent(new VendorDeactivated(Id));
    }

    /// <summary>R7: switches the vendor on again.</summary>
    public void Reactivate()
    {
        if (Status == VendorStatus.Active)
            throw new ConflictException(VendorMessages.AlreadyActive);

        Status = VendorStatus.Active;
        AddDomainEvent(new VendorReactivated(Id));
    }

    private void Apply(VendorDetails details)
    {
        Name = VendorRules.RequireName(details.Name);
        NameNormalized = VendorRules.NormalizeName(Name);
        Address = VendorRules.Optional(details.Address, 500);
        ContactPerson = VendorRules.Optional(details.ContactPerson, 200);
        Mobile = VendorRules.Mobile(details.Mobile);
        Email = VendorRules.Email(details.Email);
        Gstin = VendorRules.Gstin(details.Gstin);
        State = VendorState.RequireOrNone(details.State);
        ApplyBank(VendorRules.Bank(details.Bank));
        SetOpeningBalance(VendorRules.OpeningBalance(details.OpeningBalanceAmount));
    }

    /// <summary>R6, R16: a changed opening balance (a new vendor counts from zero) is told to the ledger through an event.</summary>
    private void SetOpeningBalance(decimal amount)
    {
        var old = OpeningBalanceAmount;
        OpeningBalanceAmount = amount;
        if (old != amount)
            AddDomainEvent(new VendorOpeningBalanceChanged(Id, Code, old, amount));
    }

    private void ApplyBank(BankAccount? bank)
    {
        BankAccountHolder = bank?.AccountHolder;
        BankName = bank?.BankName;
        BankBranch = bank?.Branch;
        BankAccountNumber = bank?.AccountNumber;
        BankIfsc = bank?.Ifsc;
    }
}
