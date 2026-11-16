using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R64-R67: money the company pays back to a school (FR-RET-09). It is complete when made and never edited; the Owner
/// cancels it with a reason and it keeps its number. Its ledger posting is <see cref="RefundLedgerEntry"/>.
/// </summary>
public class SchoolRefund : CompanyOwnedRoot
{
    public const int ReferenceMinLength = 4;
    public const int ReferenceMaxLength = 50;
    public const int RemarkMaxLength = 500;

    /// <summary>R66: the ledger document type of a refund.</summary>
    public const string DocumentType = "SCHOOL_REFUND";

    private SchoolRefund()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
        SchoolCode = string.Empty;
        SchoolName = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public DateOnly RefundDate { get; private set; }

    public Guid SchoolId { get; private set; }

    public string SchoolCode { get; private set; }

    public string SchoolName { get; private set; }

    public Guid? CreditNoteId { get; private set; }

    public string? CreditNoteNumber { get; private set; }

    public RefundMode Mode { get; private set; }

    public string? Reference { get; private set; }

    public decimal Amount { get; private set; }

    public string? Remark { get; private set; }

    public RefundStatus Status { get; private set; }

    public string? CancelReason { get; private set; }

    public DateOnly? CancelledOn { get; private set; }

    /// <summary>R64 (Q44): the code series of a financial year, <c>RF/2026-27/</c>.</summary>
    public static string NumberSeries(string financialYear) => $"RF/{financialYear}/";

    /// <summary>R64: <c>RF/2026-27/0001</c>.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R64: a refund; refused for a date in the future, an amount not above zero or with more than two decimals, a bad mode or reference or a long remark.</summary>
    public static SchoolRefund Record(RefundInput input)
    {
        Check(input);
        var refund = new SchoolRefund
        {
            Number = input.Number,
            FinancialYear = Finance.FinancialYear.Of(input.RefundDate),
            RefundDate = input.RefundDate,
            SchoolId = input.School.Id,
            SchoolCode = input.School.Code,
            SchoolName = input.School.Name,
            CreditNoteId = input.CreditNoteId,
            CreditNoteNumber = input.CreditNoteNumber,
            Mode = input.Mode,
            Reference = string.IsNullOrWhiteSpace(input.Reference) ? null : input.Reference.Trim(),
            Amount = input.Amount,
            Remark = string.IsNullOrWhiteSpace(input.Remark) ? null : input.Remark.Trim(),
            Status = RefundStatus.Active
        };
        refund.AddDomainEvent(new RefundRecorded(refund.Id, refund.SchoolId, refund.Number, refund.Amount));
        return refund;
    }

    /// <summary>R67: the Owner cancels a live refund with a reason (5-500 characters); the number stays.</summary>
    public void Cancel(string reason, DateOnly today)
    {
        if (Status == RefundStatus.Cancelled)
            throw new ConflictException(ReturnMessages.RefundAlreadyCancelled);

        var trimmed = reason?.Trim() ?? string.Empty;
        if (trimmed.Length is < SchoolOrder.ReasonMinLength or > SchoolOrder.ReasonMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidReason);

        Status = RefundStatus.Cancelled;
        CancelReason = trimmed;
        CancelledOn = today;
        AddDomainEvent(new RefundCancelled(Id, SchoolId, Number));
    }

    private static void Check(RefundInput input)
    {
        if (input.RefundDate > input.Today)
            throw new ValidationFailureException(ReturnMessages.RefundInvalidDate);
        if (input.Amount <= 0m || input.Amount > SaleMoney.MaxAmount || !SaleMoney.HasAtMostTwoDecimals(input.Amount))
            throw new ValidationFailureException(ReturnMessages.RefundInvalidAmount);
        if (!Enum.IsDefined(input.Mode))
            throw new ValidationFailureException(ReturnMessages.RefundInvalidMode);

        var reference = input.Reference?.Trim() ?? string.Empty;
        if (reference.Length > 0 && (input.Mode == RefundMode.Cash || reference.Length is < ReferenceMinLength or > ReferenceMaxLength))
            throw new ValidationFailureException(ReturnMessages.RefundInvalidReference);
        if (input.Remark?.Trim().Length > RemarkMaxLength)
            throw new ValidationFailureException(SalesMessages.InvalidText);
    }
}
