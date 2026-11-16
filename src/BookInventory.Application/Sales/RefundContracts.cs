using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R64: a new refund: the school, the amount, the mode, an optional reference, date, credit note and remark.</summary>
public sealed record RecordRefundRequest(
    [param: Required] Guid? SchoolId,
    [param: Required] decimal? Amount,
    [param: Required] RefundMode? Mode,
    [param: StringLength(SchoolRefund.ReferenceMaxLength)] string? Reference = null,
    DateOnly? RefundDate = null,
    Guid? CreditNoteId = null,
    [param: StringLength(SchoolRefund.RemarkMaxLength)] string? Remark = null);

/// <summary>R67: the Owner's reason (5-500 characters).</summary>
public sealed record CancelRefundRequest([param: Required] string? Reason);

/// <summary>R64: the refund made and its number.</summary>
public sealed record RefundMade(Guid RefundId, string Number, decimal Amount);

/// <summary>R69: the refund list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record RefundSearchQuery
{
    /// <summary>Free text over the number and the school's name or code.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public Guid? CreditNoteId { get; init; }

    public RefundStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    /// <summary>Whitelisted sort: <c>RefundDate</c>, <c>Number</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "RefundDate DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R69: one refund as it was kept (the list row and the detail are the same).</summary>
public sealed record RefundResponse(
    Guid Id,
    string Number,
    DateOnly RefundDate,
    string FinancialYear,
    Guid SchoolId,
    string SchoolCode,
    string SchoolName,
    Guid? CreditNoteId,
    string? CreditNoteNumber,
    RefundMode Mode,
    string? Reference,
    decimal Amount,
    string? Remark,
    RefundStatus Status,
    string? CancelReason,
    DateOnly? CancelledOn,
    DateTime CreatedAt);
