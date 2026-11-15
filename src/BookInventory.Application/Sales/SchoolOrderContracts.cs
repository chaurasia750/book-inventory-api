using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R3, R7: a discount as the request sends it (kind and value); the domain checks it.</summary>
public sealed record DiscountRequest([param: Required] DiscountKind? Kind, decimal? Value)
{
    public Discount ToDiscount() => Discount.Of(Kind ?? DiscountKind.None, Value ?? 0m);
}

/// <summary>R3, R20, R21: one order line: the class it is for, the line it edits on a confirmed order, the item, the chosen price version (empty: the item's default on the order date), the quantity, the item discount.</summary>
public sealed record SchoolOrderLineRequest(
    [param: Required] Guid? ItemId,
    Guid? PriceVersionId,
    [param: Required, Range(1, SchoolOrder.MaxQuantity)] int? Quantity,
    DiscountRequest? ItemDiscount = null,
    [param: StringLength(50)] string? ClassName = null,
    Guid? LineId = null);

/// <summary>R2, R3: a draft's values; the session must be the school's session of the order date or the one before or after it.</summary>
public sealed record SaveSchoolOrderRequest(
    [param: Required] Guid? SchoolId,
    DateOnly? OrderDate = null,
    [param: StringLength(SchoolOrder.SessionMaxLength)] string? Session = null,
    bool? IsGstApplied = null,
    DiscountRequest? TotalDiscount = null,
    [param: StringLength(SchoolOrder.RemarkMaxLength)] string? Remark = null,
    [param: MaxLength(SchoolOrder.MaxLines)] IReadOnlyList<SchoolOrderLineRequest>? Lines = null);

/// <summary>R6: the Owner's reason to close an order.</summary>
public sealed record CloseSchoolOrderRequest([param: Required, StringLength(SchoolOrder.ReasonMaxLength, MinimumLength = SchoolOrder.ReasonMinLength)] string Reason);

/// <summary>R15: the list filters and paging.</summary>
public sealed record SchoolOrderQuery
{
    /// <summary>Free text over the order number and the school's name and code.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public SchoolOrderStatus? Status { get; init; }

    public string? Session { get; init; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R15: one row of the order list.</summary>
public sealed record SchoolOrderSummary(
    Guid Id,
    string Number,
    Guid SchoolId,
    string SchoolCode,
    string SchoolName,
    DateOnly OrderDate,
    string Session,
    SchoolOrderStatus Status,
    decimal GrandTotal);

/// <summary>R3, R5, R7: one line as it is read: the snapshot, amounts, remaining quantity and the low-stock warning.</summary>
public sealed record SchoolOrderLineView(
    Guid Id,
    SaleItem Item,
    SalePrice Price,
    int Quantity,
    Discount ItemDiscount,
    SaleLineAmounts Amounts,
    int InvoicedQuantity,
    int RemainingQuantity,
    int? MainStock,
    bool IsLowStock,
    string? ClassName = null);

/// <summary>R15: an order with its header, totals, lines, the session choice and its invoices.</summary>
public sealed record SchoolOrderDetail
{
    public required Guid Id { get; init; }

    public required string Number { get; init; }

    public required SaleSchool School { get; init; }

    public required DateOnly OrderDate { get; init; }

    public required string Session { get; init; }

    public required bool IsGstApplied { get; init; }

    public required GstMode GstMode { get; init; }

    public required Discount TotalDiscount { get; init; }

    public string? Remark { get; init; }

    public required SchoolOrderStatus Status { get; init; }

    public string? CloseReason { get; init; }

    public required SaleTotals Totals { get; init; }

    public required IReadOnlyList<SchoolOrderLineView> Lines { get; init; }

    /// <summary>R15: the order's invoices, newest first.</summary>
    public required IReadOnlyList<InvoiceSummary> Invoices { get; init; }
}
