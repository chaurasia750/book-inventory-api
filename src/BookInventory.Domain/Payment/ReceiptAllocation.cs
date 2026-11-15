using BookInventory.Domain.Core;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R6, section 4: part of a <see cref="Receipt"/> applied to one invoice. Insert-only: the amount is signed, so a
/// reduction (a bounce, a cancelled invoice or receipt) is a new negative row and the allocated amount of an invoice is the sum.
/// </summary>
public class ReceiptAllocation : CompanyOwnedChild
{
    private ReceiptAllocation()
    {
    }

    public Guid ReceiptId { get; private set; }

    public Guid InvoiceId { get; private set; }

    public decimal Amount { get; private set; }

    public AllocationReason Reason { get; private set; }

    /// <summary>The place of this row in the receipt's history (0, 1, 2, ...): what makes "the most recently allocated" exact (R11).</summary>
    public int Sequence { get; private set; }

    internal static ReceiptAllocation Of(Guid receiptId, Guid invoiceId, decimal amount, AllocationReason reason, int sequence) =>
        new() { ReceiptId = receiptId, InvoiceId = invoiceId, Amount = amount, Reason = reason, Sequence = sequence };
}
