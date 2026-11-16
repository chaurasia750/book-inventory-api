using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R53-R59, R62: a school's return. It is complete when made (the snapshots of R59, the amounts of R7 shared as R55
/// says) and never edited; the Owner cancels it with a reason and it keeps its number. Its ledger posting is
/// <see cref="CreditNoteLedgerEntry"/>; its amounts come from <see cref="SaleCalculation"/> like an order and an invoice (D2).
/// </summary>
public class CreditNote : CompanyOwnedRoot
{
    public const int MaxLines = 500;
    public const int RemarkMaxLength = 500;

    /// <summary>R57: the stock and ledger document type of a credit note.</summary>
    public const string DocumentType = "SCHOOL_RETURN";

    private readonly List<CreditNoteLine> _lines = [];

    private CreditNote()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
        SchoolCode = string.Empty;
        SchoolName = string.Empty;
        Session = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public DateOnly NoteDate { get; private set; }

    public Guid SchoolId { get; private set; }

    public string SchoolCode { get; private set; }

    public string SchoolName { get; private set; }

    public string? SchoolState { get; private set; }

    public string? SchoolGstin { get; private set; }

    /// <summary>R72: the school's address when the credit note was made, printed as the recipient's address.</summary>
    public string? SchoolAddress { get; private set; }

    public string? CompanyState { get; private set; }

    /// <summary>R59: the company's printed identity when the credit note was made.</summary>
    public CompanyInvoiceSnapshot CompanySnapshot { get; private set; } = CompanyInvoiceSnapshot.Empty;

    public Guid? InvoiceId { get; private set; }

    public string? InvoiceNumber { get; private set; }

    /// <summary>R72: the date of the invoice the credit note refers to, as it was when the credit note was made; null for a return without an invoice.</summary>
    public DateOnly? InvoiceDate { get; private set; }

    public string Session { get; private set; }

    public bool IsGstApplied { get; private set; }

    public GstMode GstMode { get; private set; }

    public DiscountKind TotalDiscountKind { get; private set; }

    public decimal TotalDiscountValue { get; private set; }

    public SaleTotals Totals { get; private set; } = SaleTotals.Zero;

    public CreditNoteReason Reason { get; private set; }

    public string? Remark { get; private set; }

    public CreditNoteStatus Status { get; private set; }

    public string? CancelReason { get; private set; }

    public DateOnly? CancelledOn { get; private set; }

    public IReadOnlyList<CreditNoteLine> Lines => _lines.OrderBy(l => l.SortOrder).ToList();

    /// <summary>R62: what a credit note against an invoice gives back to the order; lines without an invoice give nothing.</summary>
    public OrderDelivery Delivery => new(
        [.. Lines.Where(l => l.OrderLineId is not null).Select(l => new DeliveryLine(
            l.OrderLineId!.Value,
            new SaleItem(l.ItemId, l.ItemCode, l.ItemName, l.HsnCode, l.GstRatePercent),
            new SalePrice(l.PriceVersionId, l.PriceVersionName, l.MrpAmount, l.RateAmount),
            l.Quantity,
            l.ItemDiscount,
            l.ClassName))],
        new Discount(TotalDiscountKind, TotalDiscountValue));

    /// <summary>R53 (FR-CMP-04): the code series of a financial year, <c>CN/2026-27/</c>.</summary>
    public static string NumberSeries(string financialYear) => $"CN/{financialYear}/";

    /// <summary>R53: <c>CN/2026-27/0001</c>.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R53-R56: the credit note of a return; refused for a bad date, no or too many lines, a missing remark for `Other`, or a total not above zero.</summary>
    public static CreditNote Make(CreditNoteInput input)
    {
        Check(input);
        var lines = input.Lines.Select(l => new SaleLineInput(l.Price.Rate, l.Quantity, l.ItemDiscount, l.Item.GstRatePercent)).ToList();
        var discount = TotalDiscountOf(input, lines);
        var amounts = SaleCalculation.Compute(lines, discount, input.GstMode);
        if (amounts.Totals.GrandTotal <= 0m)
            throw new ValidationFailureException(SalesMessages.TotalNotPositive);

        var note = Create(input, discount, amounts);
        note._lines.AddRange(input.Lines.Select((l, i) => CreditNoteLine.Of(note.Id, i + 1, l, amounts.Lines[i])));
        note.AddDomainEvent(new CreditNoteMade(note.Id, note.SchoolId, note.Number, amounts.Totals.GrandTotal));
        return note;
    }

    /// <summary>R58: the Owner cancels a live credit note with a reason (5-500 characters); the number stays.</summary>
    public void Cancel(string reason, DateOnly today)
    {
        if (Status == CreditNoteStatus.Cancelled)
            throw new ConflictException(ReturnMessages.AlreadyCancelled);

        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < SchoolOrder.ReasonMinLength or > SchoolOrder.ReasonMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidReason);

        Status = CreditNoteStatus.Cancelled;
        CancelReason = trimmed;
        CancelledOn = today;
        AddDomainEvent(new CreditNoteCancelled(Id, SchoolId, Number));
    }

    /// <summary>
    /// R55 (FR-RET-03): a percentage total discount of the invoice applies as it is; a rupee one is shared by the returned part of
    /// the invoice subtotal. A return without an invoice has no total discount.
    /// </summary>
    private static Discount TotalDiscountOf(CreditNoteInput input, IReadOnlyList<SaleLineInput> lines)
    {
        var total = input.Invoice?.TotalDiscount ?? Discount.None;
        if (total.Kind != DiscountKind.Amount)
            return total;

        var subtotal = SaleCalculation.Compute(lines, Discount.None, GstMode.NotApplied).Totals.Subtotal;
        var share = input.Invoice!.Subtotal == 0m ? 0m : SaleMoney.Round2(total.Value * subtotal / input.Invoice.Subtotal);
        return new Discount(DiscountKind.Amount, Math.Min(share, subtotal));
    }

    private static void Check(CreditNoteInput input)
    {
        if (input.NoteDate > input.Today || (input.Invoice is not null && input.NoteDate < input.Invoice.Date))
            throw new ValidationFailureException(ReturnMessages.InvalidDate);
        if (input.Lines.Count == 0)
            throw new ValidationFailureException(ReturnMessages.NoLines);
        if (input.Lines.Count > MaxLines)
            throw new ValidationFailureException(ReturnMessages.TooManyLines);
        if (!Enum.IsDefined(input.Reason) || input.Lines.Any(l => !Enum.IsDefined(l.Condition)))
            throw new ValidationFailureException(ReturnMessages.InvalidReason);

        var remark = input.Remark?.Trim() ?? string.Empty;
        if (remark.Length > RemarkMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidText);
        if (input.Reason == CreditNoteReason.Other && remark.Length == 0)
            throw new ValidationFailureException(ReturnMessages.RemarkRequired);
    }

    private static CreditNote Create(CreditNoteInput input, Discount discount, SaleAmounts amounts) =>
        new()
        {
            Number = input.Number,
            FinancialYear = Finance.FinancialYear.Of(input.NoteDate),
            NoteDate = input.NoteDate,
            SchoolId = input.School.Id,
            SchoolCode = input.School.Code,
            SchoolName = input.School.Name,
            SchoolState = input.School.State,
            SchoolGstin = input.SchoolGstin,
            SchoolAddress = input.School.Address,
            CompanyState = input.CompanySnapshot.State,
            CompanySnapshot = input.CompanySnapshot,
            InvoiceId = input.Invoice?.Id,
            InvoiceNumber = input.Invoice?.Number,
            InvoiceDate = input.Invoice?.Date,
            Session = input.Session,
            IsGstApplied = input.IsGstApplied,
            GstMode = input.GstMode,
            TotalDiscountKind = discount.Kind,
            TotalDiscountValue = discount.Value,
            Totals = amounts.Totals,
            Reason = input.Reason,
            Remark = string.IsNullOrWhiteSpace(input.Remark) ? null : input.Remark.Trim(),
            Status = CreditNoteStatus.Active
        };
}
