namespace BookInventory.Domain.Sales;

/// <summary>R39: an open counter takes bills; a closed counter takes none but keeps its bills. Never deleted.</summary>
public enum CounterStatus
{
    Active,

    Closed
}
