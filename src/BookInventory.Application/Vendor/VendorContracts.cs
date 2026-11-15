using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Vendor;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Vendor;

/// <summary>R5: the five bank values; all of them are given or none (the aggregate checks it).</summary>
public sealed record VendorBankRequest(string? AccountHolder, string? BankName, string? Branch, string? AccountNumber, string? Ifsc)
{
    public BankAccount ToBank() => new(AccountHolder, BankName, Branch, AccountNumber, Ifsc);
}

/// <summary>R2-R6: a new vendor. The code is generated (R11), so there is none here.</summary>
public sealed record CreateVendorRequest(
    [param: Required, StringLength(VendorRoot.NameMaxLength)] string Name,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    VendorBankRequest? Bank = null,
    decimal? OpeningBalanceAmount = null)
{
    public VendorDetails ToDetails() => new(Name, State, Address, ContactPerson, Mobile, Email, Gstin, Bank?.ToBank(), OpeningBalanceAmount ?? 0m);
}

/// <summary>R2-R6: the master data an Owner may change; not the code or the status.</summary>
public sealed record EditVendorRequest(
    [param: Required, StringLength(VendorRoot.NameMaxLength)] string Name,
    decimal OpeningBalanceAmount,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    VendorBankRequest? Bank = null)
{
    public VendorDetails ToDetails() => new(Name, State, Address, ContactPerson, Mobile, Email, Gstin, Bank?.ToBank(), OpeningBalanceAmount);
}

/// <summary>R10: the list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record VendorSearchQuery
{
    /// <summary>Free text over name, code, contact person, mobile and GSTIN.</summary>
    public string? Search { get; init; }

    public VendorStatus? Status { get; init; }

    public string? State { get; init; }

    /// <summary>Whitelisted sort: <c>Name</c>, <c>Code</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "Name ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R10: one row of the vendor list; the account number is masked (R5).</summary>
public sealed record VendorSummary(
    Guid Id,
    string Code,
    string Name,
    string? ContactPerson,
    string? Mobile,
    string? State,
    string? BankAccountMasked,
    VendorStatus Status,
    DateTime CreatedAt);

/// <summary>R5: the bank block of the vendor page, the account number in full (the Owner's view).</summary>
public sealed record VendorBankResponse(string AccountHolder, string BankName, string Branch, string AccountNumber, string Ifsc);

/// <summary>The vendor page: master data, bank details and status.</summary>
public sealed record VendorDetailResponse(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? Gstin,
    string? State,
    VendorBankResponse? Bank,
    decimal OpeningBalanceAmount,
    VendorStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
