using System.ComponentModel.DataAnnotations;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Application.Iam;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>R4: what the Super Admin gives for the Owner. The user id is generated from the company's name, never typed.</summary>
public sealed record OwnerRequest([param: Required] string FullName);

/// <summary>R3/R4: a new company and its Owner. Only the name and the Owner's full name are required (Q7).</summary>
public sealed record CreateCompanyRequest(
    [param: Required, StringLength(CompanyRoot.NameMaxLength)] string Name,
    [param: Required] OwnerRequest Owner,
    string? Address = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    string? Pan = null,
    string? State = null,
    string? Tagline = null)
{
    public CompanyDetails ToDetails() => new(Name, Address, Mobile, Email, Gstin, Pan, State, Tagline);
}

/// <summary>R3: the master data a Super Admin may change; the id and the status never change here.</summary>
public sealed record EditCompanyRequest(
    [param: Required, StringLength(CompanyRoot.NameMaxLength)] string Name,
    string? Address = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    string? Pan = null,
    string? State = null,
    string? Tagline = null)
{
    public CompanyDetails ToDetails() => new(Name, Address, Mobile, Email, Gstin, Pan, State, Tagline);
}

/// <summary>R7: a deactivation needs a reason (the aggregate checks the length).</summary>
public sealed record DeactivateCompanyRequest([param: Required] string Reason);

/// <summary>R7: a reactivation may carry a note.</summary>
public sealed record ActivateCompanyRequest(string? Note = null);

/// <summary>R9: the list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record CompanySearchQuery
{
    /// <summary>Free text over the company name.</summary>
    public string? Search { get; init; }

    public CompanyStatus? Status { get; init; }

    /// <summary>Whitelisted sort: <c>Name</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "Name ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R9: one row of the company list.</summary>
public sealed record CompanySummary(Guid Id, string Name, string? State, CompanyStatus Status, DateTime CreatedAt);

/// <summary>R8: one row of the status timeline. <see cref="From"/> is null on the creation row.</summary>
public sealed record CompanyStatusEntry(CompanyStatus? From, CompanyStatus To, string Reason, Guid? ChangedBy, DateTime ChangedAt);

/// <summary>R9/R11: the company page: master data, status, timeline and the company's accounts (read through <c>iam</c>).</summary>
public sealed record CompanyDetailResponse(
    Guid Id,
    string Name,
    string? Address,
    string? Mobile,
    string? Email,
    string? Gstin,
    string? Pan,
    string? State,
    string? Tagline,
    CompanyStatus Status,
    string? StatusReason,
    DateTime? StatusChangedAt,
    DateTime CreatedAt,
    IReadOnlyList<CompanyStatusEntry> History,
    IReadOnlyList<UserSummary> Accounts);

/// <summary>R4: the Owner's login, shown once. The temporary password is never retrievable again.</summary>
public sealed record CompanyOwnerCredentials(Guid UserId, string UserName, string TemporaryPassword);

/// <summary>R4: the answer of a company creation.</summary>
public sealed record CreatedCompanyResponse(Guid Id, string Name, CompanyStatus Status, CompanyOwnerCredentials Owner);

/// <summary>R10: what a caller with a company context may read about its company.</summary>
public sealed record CurrentCompanyResponse(Guid Id, string Name, string Slug, CompanyStatus Status, string? State);

/// <summary>R10: the answer of `companies/current`: the company of the session, or null for a Super Admin who has not entered one.</summary>
public sealed record CurrentCompanyView(CurrentCompanyResponse? Company);

/// <summary>R13: the answer of entering or leaving a company: the company now entered (null after leaving) and the access token that carries it.</summary>
public sealed record SessionCompanyResponse(CurrentCompanyResponse? Company, string AccessToken, DateTime AccessTokenExpiresAt) : IAccessTokenResponse;

/// <summary>R21: the values an Owner may change of their company; the name and the state are not part of it.</summary>
public sealed record EditCompanyProfileRequest(
    [param: StringLength(500)] string? Address = null,
    [param: StringLength(20)] string? Mobile = null,
    [param: StringLength(254)] string? Email = null,
    [param: StringLength(15)] string? Gstin = null,
    [param: StringLength(10)] string? Pan = null,
    [param: StringLength(200)] string? Tagline = null)
{
    public CompanyProfileDetails ToDetails() => new(Address, Mobile, Email, Gstin, Pan, Tagline);
}

/// <summary>R21: the session company's master data as the profile screen shows it (the name and the state are read-only there).</summary>
public sealed record CompanyProfileResponse(string Name, string? State, string? Address, string? Mobile, string? Email, string? Gstin, string? Pan, string? Tagline);

/// <summary>R19/R20: the bank account and the invoice settings as the Owner sends them; an absent part keeps the current values.</summary>
public sealed record SetCompanyInvoiceIdentityRequest(
    CompanyBankDetails? Bank = null,
    CompanyInvoiceSettings? Invoice = null)
{
    public CompanyInvoiceIdentityDetails ToDetails() => new(Bank, Invoice);
}

/// <summary>
/// R20: the invoice identity of the session's company as the API answers it: a URL for each image (null when there is
/// none), the one bank account and the invoice settings.
/// </summary>
public sealed record CompanyInvoiceIdentityResponse(
    string? LogoUrl,
    string? SignatureUrl,
    CompanyBankDetails Bank,
    CompanyInvoiceSettings Invoice);
