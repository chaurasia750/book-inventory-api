using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Item;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R43: one line of a counter bill: the item, the price version (empty: the item's default on the bill date), the quantity, the item discount.</summary>
public sealed record CounterBillLineRequest(
    [param: Required] Guid? ItemId,
    Guid? PriceVersionId,
    [param: Required, Range(1, SchoolOrder.MaxQuantity)] int? Quantity,
    DiscountRequest? ItemDiscount = null);

/// <summary>R45: one payment line: Cash or UPI, an amount above zero, an optional reference.</summary>
public sealed record CounterPaymentRequest(
    [param: Required] CounterPaymentMode? Mode,
    [param: Required] decimal? Amount,
    [param: StringLength(CounterBillPayment.ReferenceMaxLength)] string? Reference = null)
{
    public CounterPaymentInput ToInput() => new(Mode!.Value, Amount!.Value, Reference);
}

/// <summary>R42-R45: a new counter bill: the counter, the student (by id, or by name and class), the lines, the discount and the payments.</summary>
public sealed record MakeCounterBillRequest(
    [param: Required] Guid? CounterId,
    IReadOnlyList<CounterBillLineRequest> Lines,
    IReadOnlyList<CounterPaymentRequest> Payments,
    DateOnly? BillDate = null,
    Guid? StudentId = null,
    StudentInput? Student = null,
    bool? IsGstApplied = null,
    DiscountRequest? TotalDiscount = null,
    [param: StringLength(CounterBill.RemarkMaxLength)] string? Remark = null);

/// <summary>R47: the Owner's reason (5-500 characters).</summary>
public sealed record CancelCounterBillRequest([param: Required] string? Reason);

/// <summary>R46: the bill made, its total and the stock items whose Main stock went to zero or below (never a refusal, R5).</summary>
public sealed record CounterBillMade(Guid CounterBillId, string Number, decimal GrandTotal, IReadOnlyList<Guid> ItemsAtOrBelowZero);

/// <summary>R42: the number, the date and the (already resolved) values a bill is stamped with before its lines are composed.</summary>
public sealed record CounterBillStamp(string Number, DateOnly BillDate, DateOnly Today);

/// <summary>R49: the bill list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record CounterBillSearchQuery
{
    /// <summary>Free text over the bill number and the student's name.</summary>
    public string? Search { get; init; }

    public Guid? CounterId { get; init; }

    public Guid? SchoolId { get; init; }

    public string? ClassName { get; init; }

    public Guid? StudentId { get; init; }

    public CounterBillStatus? Status { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }

    /// <summary>Whitelisted sort: <c>BillDate</c>, <c>Number</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "BillDate DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R49: one row of the bill list (the student and school as the bill kept them).</summary>
public sealed record CounterBillSummary(
    Guid Id,
    string Number,
    DateOnly BillDate,
    Guid CounterId,
    Guid SchoolId,
    string SchoolName,
    Guid StudentId,
    string StudentName,
    string ClassName,
    decimal GrandTotal,
    CounterBillStatus Status,
    DateTime CreatedAt);

/// <summary>R49: one line of a bill as it was made.</summary>
public sealed record CounterBillLineResponse(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    ItemCategory Category,
    string PriceVersionName,
    decimal MrpAmount,
    int Quantity,
    DiscountKind ItemDiscountKind,
    decimal ItemDiscountValue,
    decimal GrossAmount,
    decimal ItemDiscountAmount,
    decimal LineAmount,
    decimal TaxableAmount,
    decimal CgstAmount,
    decimal SgstAmount,
    decimal IgstAmount);

/// <summary>R45, R49: one payment of a bill.</summary>
public sealed record CounterBillPaymentResponse(CounterPaymentMode Mode, decimal Amount, string? Reference);

/// <summary>R49: a bill with its student, lines, discount, GST, round-off, grand total and payments.</summary>
public sealed record CounterBillResponse(
    Guid Id,
    string Number,
    DateOnly BillDate,
    string FinancialYear,
    Guid CounterId,
    string? CounterName,
    Guid SchoolId,
    string SchoolCode,
    string SchoolName,
    string Session,
    StudentSnapshot Student,
    bool IsGstApplied,
    DiscountKind TotalDiscountKind,
    decimal TotalDiscountValue,
    decimal Subtotal,
    decimal TotalDiscountAmount,
    decimal Taxable,
    decimal Cgst,
    decimal Sgst,
    decimal Igst,
    decimal RoundOff,
    decimal GrandTotal,
    string? Remark,
    CounterBillStatus Status,
    string? CancelReason,
    DateOnly? CancelledOn,
    IReadOnlyList<CounterBillLineResponse> Lines,
    IReadOnlyList<CounterBillPaymentResponse> Payments,
    DateTime CreatedAt);
