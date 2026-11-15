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

    /// <summary>R6: the part of the receipt applied to no invoice.</summary>
    public decimal AdvanceAmount => TotalAmount - AllocatedAmount;

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
            receipt._allocations.Add(ReceiptAllocation.Of(receipt.Id, allocation.InvoiceId, allocation.Amount, AllocationReason.Allocated));

        receipt.AddDomainEvent(new ReceiptRecorded(receipt.Id, receipt.SchoolId, receipt.Number, receipt.TotalAmount));
        return receipt;
    }

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
