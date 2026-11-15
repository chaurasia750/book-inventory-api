namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R14: a receipt is recorded, or cancelled (never deleted).</summary>
public enum ReceiptStatus
{
    Recorded,
    Cancelled
}
