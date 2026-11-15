using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Customer;

/// <summary>
/// One credit customer of one company (spec customer R1-R14). A company-owned aggregate root (platform P18): its company is
/// stamped and filtered by the platform. It owns the master data (R2-R4), the opening balance and credit limit (R6, R7) and the
/// <c>ACTIVE</c>/<c>INACTIVE</c> status (R5); every rule of those is enforced here, and nothing outside changes its
/// properties. The code is generated once at creation from the company code series (platform P23) and never changes (R10).
/// Whether the school and its class exist is checked by the use case through the school directory (R4), before it calls here.
/// </summary>
public class Customer : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int CodeMaxLength = 20;
    public const int AddressMaxLength = 500;
    public const int ClassMaxLength = 50;

    /// <summary>R10: the prefix of the customer codes in the company code series (<c>CUS-0001</c>).</summary>
    public const string CodePrefix = "CUS";

    private Customer()
    {
        Code = string.Empty;
        Name = string.Empty;
        NameNormalized = string.Empty;
        Mobile = string.Empty;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    /// <summary>The name in the form it is searched in (single spaces, lower case).</summary>
    public string NameNormalized { get; private set; }

    public string Mobile { get; private set; }

    public string? Address { get; private set; }

    /// <summary>R4: a plain id, no foreign key across schemas.</summary>
    public Guid? SchoolId { get; private set; }

    public string? ClassName { get; private set; }

    public decimal OpeningBalanceAmount { get; private set; }

    public decimal? CreditLimitAmount { get; private set; }

    public CustomerStatus Status { get; private set; }

    public bool IsActive => Status == CustomerStatus.Active;

    /// <summary>R3: the stored form of a mobile number; the use case uses it to look for a customer with the same number.</summary>
    public static string NormalizeMobile(string? mobile) => CustomerRules.RequireMobile(mobile);

    /// <summary>R2-R7, R10: a new customer, active, with the generated code.</summary>
    public static Customer Create(string code, CustomerDetails details)
    {
        var customer = new Customer { Code = code, Status = CustomerStatus.Active };
        customer.Apply(details);
        customer.AddDomainEvent(new CustomerCreated(customer.Id, customer.Code, customer.Name));
        return customer;
    }

    /// <summary>R2-R7: changes the master data; the code and the status never change here.</summary>
    public void Edit(CustomerDetails details) => Apply(details);

    /// <summary>R5: switches the customer off; its data and history stay.</summary>
    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new ConflictException(CustomerMessages.AlreadyInactive);

        Status = CustomerStatus.Inactive;
        AddDomainEvent(new CustomerDeactivated(Id));
    }

    /// <summary>R5: switches the customer on again.</summary>
    public void Reactivate()
    {
        if (Status == CustomerStatus.Active)
            throw new ConflictException(CustomerMessages.AlreadyActive);

        Status = CustomerStatus.Active;
        AddDomainEvent(new CustomerReactivated(Id));
    }

    private void Apply(CustomerDetails details)
    {
        Name = CustomerRules.RequireName(details.Name);
        NameNormalized = CustomerRules.NormalizeName(Name);
        Mobile = CustomerRules.RequireMobile(details.Mobile);
        Address = CustomerRules.Optional(details.Address, AddressMaxLength);
        SchoolId = details.SchoolId;
        ClassName = CustomerRules.ClassOf(details.SchoolId, details.ClassName);
        OpeningBalanceAmount = CustomerRules.OpeningBalance(details.OpeningBalanceAmount);
        CreditLimitAmount = CustomerRules.CreditLimit(details.CreditLimitAmount);
    }
}
