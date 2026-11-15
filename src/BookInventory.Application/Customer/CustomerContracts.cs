using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Customer;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Application.Customer;

/// <summary>R2-R7: a new customer. The code is generated (R10), so there is none here.</summary>
public sealed record CreateCustomerRequest(
    [param: Required, StringLength(CustomerRoot.NameMaxLength)] string Name,
    [param: Required] string? Mobile,
    string? Address = null,
    Guid? SchoolId = null,
    string? ClassName = null,
    decimal? OpeningBalanceAmount = null,
    decimal? CreditLimitAmount = null)
{
    public CustomerDetails ToDetails(string? resolvedClassName) =>
        new(Name, Mobile, Address, SchoolId, resolvedClassName, OpeningBalanceAmount ?? 0m, CreditLimitAmount);
}

/// <summary>R2-R7: the master data an Owner may change; not the code or the status.</summary>
public sealed record EditCustomerRequest(
    [param: Required, StringLength(CustomerRoot.NameMaxLength)] string Name,
    [param: Required] string? Mobile,
    decimal OpeningBalanceAmount,
    string? Address = null,
    Guid? SchoolId = null,
    string? ClassName = null,
    decimal? CreditLimitAmount = null)
{
    public CustomerDetails ToDetails(string? resolvedClassName) =>
        new(Name, Mobile, Address, SchoolId, resolvedClassName, OpeningBalanceAmount, CreditLimitAmount);
}

/// <summary>R11: the list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record CustomerSearchQuery
{
    /// <summary>Free text over name, code and mobile.</summary>
    public string? Search { get; init; }

    public CustomerStatus? Status { get; init; }

    public Guid? SchoolId { get; init; }

    /// <summary>Whitelisted sort: <c>Name</c>, <c>Code</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "Name ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R11: one row of the customer list, with the school's name read through the school directory.</summary>
public sealed record CustomerSummary(
    Guid Id,
    string Code,
    string Name,
    string Mobile,
    Guid? SchoolId,
    string? SchoolName,
    string? ClassName,
    CustomerStatus Status,
    DateTime CreatedAt);

/// <summary>R8: one suggested customer: what a bill screen needs to choose one.</summary>
public sealed record CustomerSuggestion(Guid Id, string Code, string Name, string Mobile, string? SchoolName);

/// <summary>The customer page: master data, opening balance, credit limit and status.</summary>
public sealed record CustomerDetailResponse(
    Guid Id,
    string Code,
    string Name,
    string Mobile,
    string? Address,
    Guid? SchoolId,
    string? SchoolName,
    string? ClassName,
    decimal OpeningBalanceAmount,
    decimal? CreditLimitAmount,
    CustomerStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
