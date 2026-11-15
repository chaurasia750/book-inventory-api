using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R42-R47: one immediate sale to a student at a school counter. It is complete when made (the snapshots of R13, the
/// amounts of R7, the payments of R45) and never edited; the Owner cancels it with a reason and it keeps its number. It posts the
/// stock out and nothing to any ledger (R46); its amounts come from <see cref="SaleCalculation"/> like an order and an invoice (D2).
/// </summary>
public class CounterBill : CompanyOwnedRoot
{
    public const int MaxLines = 200;
    public const int RemarkMaxLength = 500;

    /// <summary>R42: the stock document type of a counter bill.</summary>
    public const string DocumentType = "COUNTER_BILL";

    private readonly List<CounterBillLine> _lines = [];
    private readonly List<CounterBillPayment> _payments = [];

    private CounterBill()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
        SchoolCode = string.Empty;
        SchoolName = string.Empty;
        Session = string.Empty;
        StudentName = string.Empty;
        StudentClass = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public DateOnly BillDate { get; private set; }

    public Guid CounterId { get; private set; }

    public Guid SchoolId { get; private set; }

    public string SchoolCode { get; private set; }

    public string SchoolName { get; private set; }

    public string? SchoolState { get; private set; }

    public string? SchoolGstin { get; private set; }

    public string? CompanyState { get; private set; }

    /// <summary>R13: the company's printed identity when the bill was made, kept so a reprint never changes.</summary>
    public CompanyInvoiceSnapshot CompanySnapshot { get; private set; } = CompanyInvoiceSnapshot.Empty;

    public string Session { get; private set; }

    public Guid StudentId { get; private set; }

    public string StudentName { get; private set; }

    public string StudentClass { get; private set; }

    public string? StudentSection { get; private set; }

    public string? StudentRollNumber { get; private set; }

    public string? StudentParentMobile { get; private set; }

    public bool IsGstApplied { get; private set; }

    public GstMode GstMode { get; private set; }

    public DiscountKind TotalDiscountKind { get; private set; }

    public decimal TotalDiscountValue { get; private set; }

    public SaleTotals Totals { get; private set; } = SaleTotals.Zero;

    public string? Remark { get; private set; }

    public CounterBillStatus Status { get; private set; }

    public string? CancelReason { get; private set; }

    public DateOnly? CancelledOn { get; private set; }

    public IReadOnlyList<CounterBillLine> Lines => _lines.OrderBy(l => l.SortOrder).ToList();

    public IReadOnlyList<CounterBillPayment> Payments => _payments;

    /// <summary>R42 (owner's answer Q27): the code series of a financial year, <c>CNT/2026-27/</c>.</summary>
    public static string NumberSeries(string financialYear) => $"CNT/{financialYear}/";

    /// <summary>R42: <c>CNT/2026-27/0001</c>.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R42-R45: the bill of a sale; refused for a date in the future, no lines, too many lines, a total not above zero, or payments that do not equal the total.</summary>
    public static CounterBill Make(CounterBillInput input)
    {
        Check(input);
        var amounts = SaleCalculation.Compute(
            [.. input.Lines.Select(l => new SaleLineInput(l.Price.Mrp, l.Quantity, l.ItemDiscount, l.Item.GstRatePercent))], input.TotalDiscount, input.GstMode);
        if (amounts.Totals.GrandTotal <= 0m)
            throw new ValidationFailureException(SalesMessages.TotalNotPositive);

        var bill = Create(input, amounts);
        bill._lines.AddRange(input.Lines.Select((l, i) => CounterBillLine.Of(bill.Id, i + 1, l, amounts.Lines[i])));
        bill._payments.AddRange(input.Payments.Select(p => CounterBillPayment.Of(bill.Id, p)));
        if (bill._payments.Sum(p => p.Amount) != amounts.Totals.GrandTotal || bill._payments.Select(p => p.Mode).Distinct().Count() != bill._payments.Count)
            throw new ValidationFailureException(CounterMessages.PaymentMismatch);

        bill.AddDomainEvent(new CounterBillMade(bill.Id, bill.SchoolId, bill.Number, amounts.Totals.GrandTotal));
        return bill;
    }

    /// <summary>R47: the Owner cancels a live bill with a reason (5-500 characters); the number stays.</summary>
    public void Cancel(string reason, DateOnly today)
    {
        if (Status == CounterBillStatus.Cancelled)
            throw new ConflictException(CounterMessages.BillAlreadyCancelled);

        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < SchoolOrder.ReasonMinLength or > SchoolOrder.ReasonMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidReason);

        Status = CounterBillStatus.Cancelled;
        CancelReason = trimmed;
        CancelledOn = today;
        AddDomainEvent(new CounterBillCancelled(Id, SchoolId, Number));
    }

    private static void Check(CounterBillInput input)
    {
        if (input.BillDate > input.Today)
            throw new ValidationFailureException(CounterMessages.InvalidBillDate);
        if (input.Lines.Count == 0)
            throw new ValidationFailureException(CounterMessages.BillNoLines);
        if (input.Lines.Count > MaxLines)
            throw new ValidationFailureException(CounterMessages.BillTooManyLines);
        if (input.Remark?.Trim().Length > RemarkMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidText);
    }

    private static CounterBill Create(CounterBillInput input, SaleAmounts amounts) =>
        new()
        {
            Number = input.Number,
            FinancialYear = Finance.FinancialYear.Of(input.BillDate),
            BillDate = input.BillDate,
            CounterId = input.CounterId,
            SchoolId = input.School.Id,
            SchoolCode = input.School.Code,
            SchoolName = input.School.Name,
            SchoolState = input.School.State,
            SchoolGstin = input.SchoolGstin,
            CompanyState = input.CompanySnapshot.State,
            CompanySnapshot = input.CompanySnapshot,
            Session = input.Session,
            StudentId = input.Student.Id,
            StudentName = input.Student.Name,
            StudentClass = input.Student.ClassName,
            StudentSection = input.Student.Section,
            StudentRollNumber = input.Student.RollNumber,
            StudentParentMobile = input.Student.ParentMobile,
            IsGstApplied = input.IsGstApplied,
            GstMode = input.GstMode,
            TotalDiscountKind = input.TotalDiscount.Kind,
            TotalDiscountValue = input.TotalDiscount.Value,
            Totals = amounts.Totals,
            Remark = string.IsNullOrWhiteSpace(input.Remark) ? null : input.Remark.Trim(),
            Status = CounterBillStatus.Active
        };
}
