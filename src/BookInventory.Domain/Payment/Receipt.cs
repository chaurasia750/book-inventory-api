using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R4-R9: the document of one school's payment on one date: its modes, the way the money is applied to invoices and
/// what stays as advance. Nothing is edited after it is recorded; later behaviours (cheque results, adjusting, cancelling) come
/// with their own plans. The aggregate owns its modes and allocations (platform P18).
/// </summary>
public class Receipt : CompanyOwnedRoot
{
    public const int MinModes = 1;
    public const int MaxModes = 10;
    public const int RemarkMaxLength = 500;
    public const int NumberMaxLength = 30;
    public const int ReasonMinLength = 5;
    public const int ReasonMaxLength = 500;

    private readonly List<ReceiptMode> _modes = [];
    private readonly List<ReceiptAllocation> _allocations = [];

    private Receipt()
    {
        Number = string.Empty;
        FinancialYear = string.Empty;
    }

    public string Number { get; private set; }

    public string FinancialYear { get; private set; }

    public Guid SchoolId { get; private set; }

    public DateOnly ReceiptDate { get; private set; }

    public decimal TotalAmount { get; private set; }

    public ReceiptStatus Status { get; private set; }

    public string? Remark { get; private set; }

    public Guid ClientRequestId { get; private set; }

    public IReadOnlyList<ReceiptMode> Modes => _modes.Where(m => !m.IsDeleted).ToList();

    public IReadOnlyList<ReceiptAllocation> Allocations => _allocations.Where(a => !a.IsDeleted).ToList();

    /// <summary>The amount applied to invoices now (the sum of the signed allocation rows).</summary>
    public decimal AllocatedAmount => Allocations.Sum(a => a.Amount);

    /// <summary>R10: the amount of the cheques that bounced.</summary>
    public decimal BouncedAmount => Modes.Where(m => m.ChequeStatus == ChequeStatus.Bounced).Sum(m => m.Amount);

    /// <summary>R6, R11: the part of the receipt applied to no invoice (not counting what bounced).</summary>
    public decimal AdvanceAmount => TotalAmount - AllocatedAmount - BouncedAmount;

    /// <summary>R8: the prefix of the receipt numbers of a financial year (<c>RCP/2026-27/</c>).</summary>
    public static string NumberSeries(string financialYear) => $"RCP/{financialYear}/";

    /// <summary>R8: the receipt number from the company counter value.</summary>
    public static string NumberOf(string financialYear, int number) => $"{NumberSeries(financialYear)}{number:D4}";

    /// <summary>R4-R9: records a receipt; the allocation is planned from the school's open invoices (R6).</summary>
    public static Receipt Record(ReceiptDraft draft)
    {
        RequireValid(draft);
        var receipt = new Receipt
        {
            Number = draft.Number,
            FinancialYear = draft.FinancialYear,
            SchoolId = draft.SchoolId,
            ReceiptDate = draft.ReceiptDate,
            Status = ReceiptStatus.Recorded,
            Remark = string.IsNullOrWhiteSpace(draft.Remark) ? null : draft.Remark.Trim(),
            ClientRequestId = draft.ClientRequestId
        };
        receipt._modes.AddRange(draft.Modes.Select(m => ReceiptMode.Of(receipt.Id, m)));
        receipt.TotalAmount = PaymentMoney.Require(receipt._modes.Sum(m => m.Amount));
        foreach (var allocation in AllocationPlanner.Plan(receipt.TotalAmount, draft.OpenInvoices, draft.Allocations))
            receipt._allocations.Add(ReceiptAllocation.Of(receipt.Id, allocation.InvoiceId, allocation.Amount, AllocationReason.Allocated, receipt._allocations.Count));

        receipt.AddDomainEvent(new ReceiptRecorded(receipt.Id, receipt.SchoolId, receipt.Number, receipt.TotalAmount));
        return receipt;
    }

    /// <summary>R10: a pending cheque cleared on <paramref name="date"/> (not before the cheque date, not in the future); the use case posts the transfer.</summary>
    public ReceiptMode ClearCheque(Guid modeId, DateOnly date, DateOnly today)
    {
        RequireRecorded();
        var mode = PendingCheque(modeId);
        if (date > today || date < mode.ChequeDate)
            throw new ValidationFailureException(PaymentMessages.InvalidResultDate);

        mode.Clear(date);
        AddDomainEvent(new ChequeCleared(Id, mode.Id, SchoolId, mode.Amount));
        return mode;
    }

    /// <summary>R10, R11: a pending cheque bounced; its amount is taken back from the advance first, then from the newest allocation; the use case posts the reversal.</summary>
    public ReceiptMode BounceCheque(Guid modeId, DateOnly date, DateOnly today, string? reason)
    {
        RequireRecorded();
        var mode = PendingCheque(modeId);
        var cleaned = reason?.Trim() ?? string.Empty;
        if (date > today)
            throw new ValidationFailureException(PaymentMessages.InvalidResultDate);
        if (cleaned.Length is < ReasonMinLength or > ReasonMaxLength)
            throw new ValidationFailureException(PaymentMessages.InvalidReason);

        TakeBack(mode.Amount);
        mode.Bounce(date, cleaned);
        AddDomainEvent(new ChequeBounced(Id, mode.Id, SchoolId, mode.Amount));
        return mode;
    }

    /// <summary>R12: the invoice was cancelled; what this receipt applied to it becomes advance (a negative allocation row).</summary>
    public void ReleaseInvoice(Guid invoiceId)
    {
        var applied = Allocations.Where(a => a.InvoiceId == invoiceId).Sum(a => a.Amount);
        if (applied > 0m)
            _allocations.Add(ReceiptAllocation.Of(Id, invoiceId, -applied, AllocationReason.InvoiceCancelled, _allocations.Count));
    }

    private void RequireRecorded()
    {
        if (Status != ReceiptStatus.Recorded)
            throw new ConflictException(PaymentMessages.ReceiptNotRecorded);
    }

    private ReceiptMode PendingCheque(Guid modeId)
    {
        var mode = Modes.FirstOrDefault(m => m.Id == modeId && m.ModeType == ModeType.Cheque) ?? throw new NotFoundException(PaymentMessages.ChequeNotFound);
        return mode.ChequeStatus == ChequeStatus.Pending ? mode : throw new ConflictException(PaymentMessages.ChequeResultSet);
    }

    /// <summary>R11: takes <paramref name="amount"/> back, the advance first, then the most recently allocated invoices.</summary>
    private void TakeBack(decimal amount)
    {
        var left = amount - Math.Min(Math.Max(AdvanceAmount, 0m), amount);
        foreach (var (invoiceId, net) in NewestAllocatedFirst())
        {
            if (left <= 0m)
                break;

            var take = Math.Min(net, left);
            _allocations.Add(ReceiptAllocation.Of(Id, invoiceId, -take, AllocationReason.Bounced, _allocations.Count));
            left -= take;
        }
    }

    /// <summary>The invoices with something still applied, the most recently allocated first (by the rows' sequence, which survives a reload).</summary>
    private IEnumerable<(Guid InvoiceId, decimal Net)> NewestAllocatedFirst() =>
        _allocations
            .Where(a => a.Amount > 0m)
            .GroupBy(a => a.InvoiceId)
            .Select(g => (InvoiceId: g.Key, Last: g.Max(a => a.Sequence), Net: _allocations.Where(a => a.InvoiceId == g.Key).Sum(a => a.Amount)))
            .Where(x => x.Net > 0m)
            .OrderByDescending(x => x.Last)
            .Select(x => (x.InvoiceId, x.Net));

    private static void RequireValid(ReceiptDraft draft)
    {
        if (draft.ClientRequestId == Guid.Empty)
            throw new ValidationFailureException(PaymentMessages.InvalidRequestId);
        if (draft.ReceiptDate > draft.Today)
            throw new ValidationFailureException(PaymentMessages.InvalidDate);
        if (draft.Modes.Count is < MinModes or > MaxModes)
            throw new ValidationFailureException(PaymentMessages.InvalidModes);
        if (draft.Remark is { Length: > RemarkMaxLength })
            throw new ValidationFailureException(PaymentMessages.InvalidRemark);
    }
}
