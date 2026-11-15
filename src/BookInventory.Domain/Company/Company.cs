using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Company;

/// <summary>
/// One business using the system (spec company R1-R20). The aggregate root that owns the master data (R3), the
/// <c>ACTIVE</c>/<c>INACTIVE</c> status with its reason (R7), the append-only status timeline (R8) and the printed
/// invoice identity (R17-R20); every rule of those is enforced here, and nothing outside changes its properties.
/// <list type="bullet">
/// <item><b>Name (R3).</b> Required, 2-150 characters, unique ignoring case and surrounding or repeated spaces: the
/// normalised form is what the unique index holds.</item>
/// <item><b>Owner user id (R4, D4).</b> The login of the company's Owner is generated from the name by
/// <see cref="OwnerUserId"/>; <c>iam</c> only checks that it is free.</item>
/// <item><b>Status (R7).</b> Deactivating needs a reason of 5-500 characters; each change adds one timeline row. A
/// company is never deleted.</item>
/// <item><b>Invoice identity (R17-R20).</b> One logo, one signature (PNG or JPEG up to 2 MB, told apart by their bytes),
/// one bank account and the invoice settings (terms, footer, whether the GSTIN prints).</item>
/// </list>
/// </summary>
public class Company : AggregateRoot, IAuditCompanyScope
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    /// <summary>R17/R18: the largest logo or signature image the company may store (2 MB).</summary>
    public const int ImageMaxBytes = 2 * 1024 * 1024;
    public const int TermsMaxLength = 4000;
    public const int FooterMaxLength = 200;
    public const int BankAccountHolderMaxLength = 150;
    public const int BankNameMaxLength = 150;
    public const int BankAccountNumberMaxLength = 50;
    public const int BankIfscMaxLength = 11;
    public const int BankBranchMaxLength = 150;
    public const int UpiIdMaxLength = 100;

    private readonly List<CompanyStatusHistory> _statusHistory = [];

    private Company()
    {
        Name = string.Empty;
        NameNormalized = string.Empty;
    }

    public string Name { get; private set; }

    /// <summary>The name in the form the unique index holds (trimmed, single spaces, lower case).</summary>
    public string NameNormalized { get; private set; }

    public string? Address { get; private set; }

    public string? Mobile { get; private set; }

    public string? Email { get; private set; }

    public string? Gstin { get; private set; }

    public string? Pan { get; private set; }

    public string? State { get; private set; }

    public string? Tagline { get; private set; }

    /// <summary>R17: the logo printed in the header of every document; null when none. Never written to the audit trail.</summary>
    [AuditSecret]
    public byte[]? Logo { get; private set; }

    public string? LogoContentType { get; private set; }

    /// <summary>R18: the signature printed above the signatory line; null when none. Never written to the audit trail.</summary>
    [AuditSecret]
    public byte[]? Signature { get; private set; }

    public string? SignatureContentType { get; private set; }

    /// <summary>R19: the one bank account printed at the bottom of the company's documents.</summary>
    public string? BankAccountHolder { get; private set; }

    public string? BankName { get; private set; }

    public string? BankAccountNumber { get; private set; }

    public string? BankIfsc { get; private set; }

    public string? BankBranch { get; private set; }

    public string? UpiId { get; private set; }

    /// <summary>R20: the terms and conditions printed on the company's documents.</summary>
    public string? InvoiceTerms { get; private set; }

    public string? InvoiceFooter { get; private set; }

    /// <summary>R20: whether the GSTIN prints on the company's documents; true by default.</summary>
    public bool PrintGstin { get; private set; } = true;

    /// <summary>R23: whether the company's orders and invoices may carry a discount per line; off by default.</summary>
    public bool UsesItemDiscount { get; private set; }

    public CompanyStatus Status { get; private set; }

    /// <summary>Why the company was last deactivated, or the note of a reactivation. Null when there is none.</summary>
    public string? StatusReason { get; private set; }

    public DateTime? StatusChangedAt { get; private set; }

    public IReadOnlyList<CompanyStatusHistory> StatusHistory => _statusHistory;

    public bool IsActive => Status == CompanyStatus.Active;

    /// <summary>A5: the audit entries of a company belong to that company.</summary>
    Guid? IAuditCompanyScope.AuditCompanyId => Id;

    /// <summary>R4/D4: the Owner's user id, generated from the company's name (see <see cref="CompanyOwnerUserId"/>).</summary>
    public string OwnerUserId => CompanyOwnerUserId.From(Name);

    /// <summary>R15: the company's URL segment, derived from its name (nothing is stored, a rename changes it).</summary>
    public string Slug => CompanyRules.Slug(Name);

    public static string NormalizeName(string? name) => CompanyRules.NormalizeName(name);

    /// <summary>R15: the first URL segments that belong to the application itself, never to a company.</summary>
    public static IReadOnlySet<string> ReservedSlugs => CompanyRules.ReservedSlugs;

    /// <summary>R15: the URL segment a name would get; used to compare two names.</summary>
    public static string SlugOf(string? name) => CompanyRules.Slug(name);

    /// <summary>R3/R7: a new company is active at once and its timeline starts with the creation row.</summary>
    public static Company Create(CompanyDetails details, Guid actorId, DateTime utcNow)
    {
        var company = new Company { Status = CompanyStatus.Active, StatusChangedAt = utcNow };
        company.Apply(details);
        company._statusHistory.Add(CompanyStatusHistory.Of(company.Id, null, CompanyStatus.Active, string.Empty, actorId, utcNow));
        company.AddDomainEvent(new CompanyCreated(company.Id, company.Name));
        return company;
    }

    /// <summary>R3: changes the master data; the id and the status never change here.</summary>
    public void Edit(CompanyDetails details) => Apply(details);

    /// <summary>R21: the Owner's edit: the address, contact, GSTIN, PAN and tagline go through the same rules as the master data; the name and the state stay.</summary>
    public void EditProfile(CompanyProfileDetails profile) =>
        Apply(new CompanyDetails(Name, profile.Address, profile.Mobile, profile.Email, profile.Gstin, profile.Pan, State, profile.Tagline));

    /// <summary>R17: stores the logo after checking it is a PNG or JPEG up to 2 MB; replaces any current logo.</summary>
    public void SetLogo(byte[]? content)
    {
        LogoContentType = CompanyImageRules.Require(content, CompanyMessages.LogoInvalid);
        Logo = content;
    }

    /// <summary>R17: removes the logo; a company with no logo prints none and stays valid.</summary>
    public void RemoveLogo()
    {
        Logo = null;
        LogoContentType = null;
    }

    /// <summary>R18: stores the signature after checking it is a PNG or JPEG up to 2 MB; replaces any current signature.</summary>
    public void SetSignature(byte[]? content)
    {
        SignatureContentType = CompanyImageRules.Require(content, CompanyMessages.SignatureInvalid);
        Signature = content;
    }

    /// <summary>R18: removes the signature; the signatory line stays empty.</summary>
    public void RemoveSignature()
    {
        Signature = null;
        SignatureContentType = null;
    }

    /// <summary>R19/R20: sets the one bank account (checked by the shared <see cref="BankAccount"/> rule) and the invoice settings.</summary>
    public void SetInvoiceIdentity(CompanyInvoiceIdentityDetails details)
    {
        var account = BankAccount.Validated(
            new BankAccount(details.Bank.AccountHolder, details.Bank.BankName, details.Bank.Branch, details.Bank.AccountNumber, details.Bank.Ifsc),
            CompanyMessages.InvalidBank);
        BankAccountHolder = account?.AccountHolder;
        BankName = account?.BankName;
        BankAccountNumber = account?.AccountNumber;
        BankIfsc = account?.Ifsc;
        BankBranch = account?.Branch;
        UpiId = CompanyRules.Optional(details.Bank.UpiId, UpiIdMaxLength);
        InvoiceTerms = CompanyRules.Optional(details.Invoice.Terms, TermsMaxLength);
        InvoiceFooter = CompanyRules.Optional(details.Invoice.Footer, FooterMaxLength);
        PrintGstin = details.Invoice.PrintGstin;
        UsesItemDiscount = details.Invoice.UsesItemDiscount;
    }

    /// <summary>R7/R8: switches the company off. A reason is required; the timeline gets one row.</summary>
    public void Deactivate(string? reason, Guid actorId, DateTime utcNow)
    {
        if (Status == CompanyStatus.Inactive)
            throw new ConflictException(CompanyMessages.AlreadyInactive);

        var validated = CompanyRules.RequireReason(reason);
        ChangeStatus(CompanyStatus.Inactive, validated, actorId, utcNow);
        AddDomainEvent(new CompanyDeactivated(Id, validated));
    }

    /// <summary>R7/R8: switches the company on again; the note is optional.</summary>
    public void Reactivate(string? note, Guid actorId, DateTime utcNow)
    {
        if (Status == CompanyStatus.Active)
            throw new ConflictException(CompanyMessages.AlreadyActive);

        ChangeStatus(CompanyStatus.Active, CompanyRules.OptionalReason(note), actorId, utcNow);
        AddDomainEvent(new CompanyReactivated(Id));
    }

    private void ChangeStatus(CompanyStatus to, string reason, Guid actorId, DateTime utcNow)
    {
        _statusHistory.Add(CompanyStatusHistory.Of(Id, Status, to, reason, actorId, utcNow));
        Status = to;
        StatusReason = reason.Length == 0 ? null : reason;
        StatusChangedAt = utcNow;
    }

    private void Apply(CompanyDetails details)
    {
        Name = CompanyRules.RequireName(details.Name);
        CompanyRules.RequireSlug(Name);
        NameNormalized = CompanyRules.NormalizeName(Name);
        Address = CompanyRules.Optional(details.Address, 500);
        Mobile = ContactRules.Mobile(details.Mobile, CompanyMessages.InvalidMobile);
        Email = ContactRules.Email(details.Email, CompanyMessages.InvalidEmail);
        Gstin = ContactRules.Gstin(details.Gstin, CompanyMessages.InvalidGstin);
        Pan = ContactRules.Pan(details.Pan, CompanyMessages.InvalidPan);
        State = CompanyRules.Optional(details.State, 100);
        Tagline = CompanyRules.Optional(details.Tagline, 200);
    }
}
