using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R8: the quantity of one order line delivered now.</summary>
public sealed record InvoiceLineRequest([param: Required] Guid? OrderLineId, [param: Required, Range(0, SchoolOrder.MaxQuantity)] int? Quantity);

/// <summary>R8, R9: the invoice of a confirmed order: the date (today when empty) and the quantities (every remaining quantity when empty).</summary>
public sealed record MakeInvoiceRequest(DateOnly? InvoiceDate = null, [param: MaxLength(SchoolOrder.MaxLines)] IReadOnlyList<InvoiceLineRequest>? Lines = null);

/// <summary>R12: the Owner's reason to cancel an invoice.</summary>
public sealed record CancelInvoiceRequest([param: Required, StringLength(SchoolOrder.ReasonMaxLength, MinimumLength = SchoolOrder.ReasonMinLength)] string Reason);

/// <summary>R5, R11: the invoice made and the items whose Main stock went to zero or below.</summary>
public sealed record InvoiceMade(Guid InvoiceId, string Number, decimal GrandTotal, IReadOnlyList<Guid> ItemsAtOrBelowZero);

/// <summary>R15: the invoice list filters and paging.</summary>
public sealed record InvoiceQuery
{
    /// <summary>Free text over the invoice and order numbers and the school's name and code.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public InvoiceStatus? Status { get; init; }

    public string? FinancialYear { get; init; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R15: one row of the invoice list.</summary>
public sealed record InvoiceSummary(
    Guid Id,
    string Number,
    DateOnly InvoiceDate,
    Guid SchoolOrderId,
    string OrderNumber,
    Guid SchoolId,
    string SchoolCode,
    string SchoolName,
    InvoiceStatus Status,
    decimal GrandTotal);

/// <summary>R15: one invoice line as it is read.</summary>
public sealed record InvoiceLineView(Guid Id, SaleItem Item, SalePrice Price, int Quantity, Discount ItemDiscount, SaleLineAmounts Amounts);

/// <summary>R13, R15: an invoice with its snapshot, lines and totals.</summary>
public sealed record InvoiceDetail
{
    public required InvoiceSummary Summary { get; init; }

    public required string FinancialYear { get; init; }

    public required SaleSchool School { get; init; }

    public string? SchoolGstin { get; init; }

    public string? CompanyState { get; init; }

    public required string Session { get; init; }

    public required bool IsGstApplied { get; init; }

    public required GstMode GstMode { get; init; }

    public required Discount TotalDiscount { get; init; }

    public required SaleTotals Totals { get; init; }

    public string? CancelReason { get; init; }

    public DateOnly? CancelledOn { get; init; }

    public required IReadOnlyList<InvoiceLineView> Lines { get; init; }
}
