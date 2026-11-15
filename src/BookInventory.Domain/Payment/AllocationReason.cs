namespace BookInventory.Domain.Payment;

/// <summary>Spec payment section 4: why an allocation row exists; a reduction is a new negative row, never an edit.</summary>
public enum AllocationReason
{
    Allocated,
    Bounced,
    InvoiceCancelled,
    ReceiptCancelled,
    Adjusted
}
