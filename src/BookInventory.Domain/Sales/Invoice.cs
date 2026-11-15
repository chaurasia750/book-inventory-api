using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R8-R13: an invoice made from a confirmed order. It is complete when made (the snapshot of R13, the amounts of R7)
/// and never edited; the Owner cancels it with a reason and it keeps its number. Its ledger posting is <see cref="InvoiceLedgerEntry"/>.
/// </summary>
public class Invoice : CompanyOwnedRoot
{
    private readonly List<InvoiceLine> _lines = [];

    private Invoice()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
        OrderNumber = string.Empty;
        SchoolCode = string.Empty;
        SchoolName = string.Empty;
        Session = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public DateOnly InvoiceDate { get; private set; }

    public Guid SchoolOrderId { get; private set; }

    public string OrderNumber { get; private set; }

    public Guid SchoolId { get; private set; }

    public string SchoolCode { get; private set; }

    public string SchoolName { get; private set; }

    public string? SchoolState { get; private set; }

    /// <summary>R31: the school's address as it was when the invoice was made (the "Bill To" block).</summary>
    public string? SchoolAddress { get; private set; }

    public string? SchoolGstin { get; private set; }

    public string? CompanyState { get; private set; }

    /// <summary>R33, D7: the company's printed identity at the time of issue (bank account, terms, footer, name, address, GSTIN).</summary>
    public CompanyInvoiceSnapshot CompanySnapshot { get; private set; } = CompanyInvoiceSnapshot.Empty;

    public string Session { get; private set; }

    public bool IsGstApplied { get; private set; }

    public GstMode GstMode { get; private set; }

    /// <summary>R34: the order's payment type when the invoice was made; with GST off it decides Cash Memo or Credit Memo (R31).</summary>
    public PaymentType PaymentType { get; private set; }

    public DiscountKind TotalDiscountKind { get; private set; }

    public decimal TotalDiscountValue { get; private set; }

    public SaleTotals Totals { get; private set; } = SaleTotals.Zero;

    public InvoiceStatus Status { get; private set; }

    public string? CancelReason { get; private set; }

    public DateOnly? CancelledOn { get; private set; }

    public IReadOnlyList<InvoiceLine> Lines => _lines.OrderBy(l => l.SortOrder).ToList();

    /// <summary>R12: what this invoice took from its order, to give it back on a cancel.</summary>
    public OrderDelivery Delivery => new([.. Lines.Select(l => l.Delivery)], new Discount(TotalDiscountKind, TotalDiscountValue));

    /// <summary>R10 (owner's answer Q3): the code series of a financial year, <c>INV/2026-27/</c>.</summary>
    public static string NumberSeries(string financialYear) => $"INV/{financialYear}/";

    /// <summary>R10: <c>INV/2026-27/0001</c>.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R8-R10, R13: the invoice of a delivery; refused for a date in the future or before the order, or a total not above zero.</summary>
    public static Invoice Issue(InvoiceIssue issue)
    {
        var order = issue.Order;
        if (issue.InvoiceDate > issue.Today || issue.InvoiceDate < order.OrderDate)
            throw new ValidationFailureException(SalesMessages.InvalidDate);

        var delivery = issue.Delivery;
        var amounts = SaleCalculation.Compute(
            [.. delivery.Lines.Select(d => new SaleLineInput(d.Price.Rate, d.Quantity, d.ItemDiscount, d.Item.GstRatePercent))], delivery.TotalDiscount, order.GstMode);
        if (amounts.Totals.GrandTotal <= 0m)
            throw new ValidationFailureException(SalesMessages.TotalNotPositive);

        var invoice = new Invoice
        {
            Number = issue.Number,
            FinancialYear = Finance.FinancialYear.Of(issue.InvoiceDate),
            InvoiceDate = issue.InvoiceDate,
            SchoolOrderId = order.Id,
            OrderNumber = order.Number,
            SchoolId = issue.School.Id,
            SchoolCode = issue.School.Code,
            SchoolName = issue.School.Name,
            SchoolState = issue.School.State,
            SchoolAddress = issue.School.Address,
            SchoolGstin = issue.SchoolGstin,
            CompanyState = issue.CompanyState,
            CompanySnapshot = issue.CompanySnapshot,
            Session = order.Session,
            IsGstApplied = order.IsGstApplied,
            GstMode = order.GstMode,
            PaymentType = order.PaymentType,
            TotalDiscountKind = delivery.TotalDiscount.Kind,
            TotalDiscountValue = delivery.TotalDiscount.Value,
            Totals = amounts.Totals,
            Status = InvoiceStatus.Issued
        };
        invoice._lines.AddRange(delivery.Lines.Select((d, i) => InvoiceLine.Of(invoice.Id, i + 1, d, amounts.Lines[i])));
        invoice.AddDomainEvent(new InvoiceIssued(invoice.Id, invoice.SchoolId, invoice.Number, amounts.Totals.GrandTotal));
        return invoice;
    }

    /// <summary>R12 (FR-ORD-14): the Owner cancels an issued invoice with a reason (5-500 characters).</summary>
    public void Cancel(string reason, DateOnly today)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new ConflictException(SalesMessages.AlreadyCancelled);
        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < SchoolOrder.ReasonMinLength or > SchoolOrder.ReasonMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidReason);

        Status = InvoiceStatus.Cancelled;
        CancelReason = trimmed;
        CancelledOn = today;
        AddDomainEvent(new InvoiceCancelled(Id, SchoolId, Number));
    }
}
