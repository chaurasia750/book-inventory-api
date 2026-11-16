namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R67: a refund is active, or cancelled (it keeps its number).</summary>
public enum RefundStatus
{
    Active,
    Cancelled
}
