using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Core;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Application.School;

/// <summary>R2-R7: a new school. The code is generated (R15), so there is none here; the classes default when absent (R6).</summary>
public sealed record CreateSchoolRequest(
    [param: Required, StringLength(SchoolRoot.NameMaxLength)] string Name,
    [param: Required] SessionType? SessionType,
    SaleType? SaleType = null,
    bool? IsGstDefault = null,
    decimal? OpeningBalanceAmount = null,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    PaymentType? PaymentType = null,
    IReadOnlyList<string>? Classes = null)
{
    public SchoolDetails ToDetails() => new(
        Name, SessionType!.Value, SaleType ?? Domain.School.SaleType.Both, IsGstDefault ?? false, OpeningBalanceAmount ?? 0m,
        State, Address, ContactPerson, Mobile, Email, Gstin, PaymentType ?? Domain.Core.PaymentType.Credit);
}

/// <summary>R2-R5, R7: the master data an Owner may change; not the code, the status or the classes.</summary>
public sealed record EditSchoolRequest(
    [param: Required, StringLength(SchoolRoot.NameMaxLength)] string Name,
    [param: Required] SessionType? SessionType,
    [param: Required] SaleType? SaleType,
    bool IsGstDefault,
    decimal OpeningBalanceAmount,
    string? State = null,
    string? Address = null,
    string? ContactPerson = null,
    string? Mobile = null,
    string? Email = null,
    string? Gstin = null,
    PaymentType? PaymentType = null)
{
    public SchoolDetails ToDetails() => new(
        Name, SessionType!.Value, SaleType!.Value, IsGstDefault, OpeningBalanceAmount,
        State, Address, ContactPerson, Mobile, Email, Gstin, PaymentType ?? Domain.Core.PaymentType.Credit);
}

/// <summary>R6: the whole class list in order.</summary>
public sealed record ReplaceClassesRequest([param: Required] IReadOnlyList<string> Classes);

/// <summary>R11: the list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record SchoolSearchQuery
{
    /// <summary>Free text over name, code, contact person and mobile.</summary>
    public string? Search { get; init; }

    public SchoolStatus? Status { get; init; }

    public SaleType? SaleType { get; init; }

    public SessionType? SessionType { get; init; }

    public string? State { get; init; }

    /// <summary>Whitelisted sort: <c>Name</c>, <c>Code</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "Name ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R11: one row of the school list.</summary>
public sealed record SchoolSummary(
    Guid Id,
    string Code,
    string Name,
    string? ContactPerson,
    string? Mobile,
    string State,
    SessionType SessionType,
    SaleType SaleType,
    SchoolStatus Status,
    DateTime CreatedAt);

/// <summary>R6: one class of the list, in order.</summary>
public sealed record SchoolClassResponse(Guid Id, string Name, int SortOrder);

/// <summary>The school page: master data, status and the class list.</summary>
public sealed record SchoolDetailResponse(
    Guid Id,
    string Code,
    string Name,
    string? Address,
    string? ContactPerson,
    string? Mobile,
    string? Email,
    string? Gstin,
    string State,
    SessionType SessionType,
    SaleType SaleType,
    PaymentType PaymentType,
    bool IsGstDefault,
    decimal OpeningBalanceAmount,
    SchoolStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<SchoolClassResponse> Classes);
