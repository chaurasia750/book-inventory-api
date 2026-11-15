namespace BookInventory.Domain.Sales;

/// <summary>R47: a counter bill is live, or cancelled by the Owner (it keeps its number).</summary>
public enum CounterBillStatus
{
    Active,

    Cancelled
}
