namespace BookInventory.Domain.Stock;

/// <summary>Spec stock R3: the three stocks an item's quantity is kept in, apart from each other (BR-STK-02).</summary>
public enum StockType
{
    Main,
    Damaged,
    Sample
}
