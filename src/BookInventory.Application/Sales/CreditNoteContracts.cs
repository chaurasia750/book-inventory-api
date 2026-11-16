using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// R54: one returned line. With an invoice: the invoice line, the quantity and the condition. Without one: the item, the quantity,
/// the condition and optionally a changed rate and item discount.
/// </summary>
public sealed record CreditNoteLineRequest(
    [param: Required, Range(1, SchoolOrder.MaxQuantity)] int? Quantity,
    [param: Required] ReturnCondition? Condition,
    Guid? InvoiceLineId = null,
    Guid? ItemId = null,
    decimal? Rate = null,
    DiscountRequest? Discount = null);

/// <summary>R53-R56: a new credit note: the school, the optional invoice, the reason and the lines.</summary>
public sealed record MakeCreditNoteRequest(
    [param: Required] Guid? SchoolId,
    [param: Required] CreditNoteReason? Reason,
    IReadOnlyList<CreditNoteLineRequest> Lines,
    Guid? InvoiceId = null,
    DateOnly? NoteDate = null,
    bool? IsGstApplied = null,
    [param: StringLength(CreditNote.RemarkMaxLength)] string? Remark = null);

/// <summary>R58: the Owner's reason (5-500 characters).</summary>
public sealed record CancelCreditNoteRequest([param: Required] string? Reason);

/// <summary>R57: the credit note made, its total and the items returned beyond what was sold (a warning, never a refusal, R54).</summary>
public sealed record CreditNoteMade(Guid CreditNoteId, string Number, decimal GrandTotal, IReadOnlyList<Guid> OverReturnedItemIds);

/// <summary>R61: the credit note list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record CreditNoteSearchQuery
{
    /// <summary>Free text over the number and the school's name or code.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public Guid? InvoiceId { get; init; }

    public CreditNoteStatus? Status { get; init; }

    public CreditNoteReason? Reason { get; init; }

    public string? Session { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    /// <summary>Whitelisted sort: <c>NoteDate</c>, <c>Number</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "NoteDate DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R61: one row of the credit note list.</summary>
public sealed record CreditNoteSummary(
    Guid Id,
    string Number,
    DateOnly NoteDate,
    Guid SchoolId,
    string SchoolName,
    string SchoolCode,
    string? InvoiceNumber,
    CreditNoteReason Reason,
    decimal GrandTotal,
    CreditNoteStatus Status,
    DateTime CreatedAt);

/// <summary>R61: one line of a credit note as it was kept.</summary>
public sealed record CreditNoteLineResponse(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string PriceVersionName,
    decimal MrpAmount,
    decimal RateAmount,
    int Quantity,
    ReturnCondition Condition,
    DiscountKind ItemDiscountKind,
    decimal ItemDiscountValue,
    decimal GrossAmount,
    decimal ItemDiscountAmount,
    decimal LineAmount,
    decimal TotalDiscountShareAmount,
    decimal TaxableAmount,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal IgstAmount);

/// <summary>R61: one credit note with its lines, totals, reason and invoice.</summary>
public sealed record CreditNoteResponse(
    Guid Id,
    string Number,
    DateOnly NoteDate,
    string FinancialYear,
    Guid SchoolId,
    string SchoolCode,
    string SchoolName,
    Guid? InvoiceId,
    string? InvoiceNumber,
    string Session,
    bool IsGstApplied,
    CreditNoteReason Reason,
    string? Remark,
    decimal Subtotal,
    decimal TotalDiscount,
    decimal Taxable,
    decimal Cgst,
    decimal Sgst,
    decimal Igst,
    decimal RoundOff,
    decimal GrandTotal,
    CreditNoteStatus Status,
    string? CancelReason,
    DateOnly? CancelledOn,
    IReadOnlyList<CreditNoteLineResponse> Lines,
    DateTime CreatedAt);

/// <summary>R54: an invoice line with what was sold, what live credit notes returned and what can still be returned.</summary>
public sealed record ReturnableLine(
    Guid InvoiceLineId,
    Guid ItemId,
    string ItemCode,
    string ItemName,
    string? ClassName,
    decimal RateAmount,
    int SoldQuantity,
    int ReturnedQuantity)
{
    public int ReturnableQuantity => Math.Max(0, SoldQuantity - ReturnedQuantity);
}

/// <summary>R54, R61: the invoice's returnable lines and the credit notes made against it.</summary>
public sealed record InvoiceReturns(Guid InvoiceId, string InvoiceNumber, IReadOnlyList<ReturnableLine> Lines, IReadOnlyList<CreditNoteSummary> CreditNotes);
