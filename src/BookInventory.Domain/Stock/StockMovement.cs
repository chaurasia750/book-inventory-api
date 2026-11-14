namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R4, R12: one movement to post on an item's stock: the stock type, the change, the movement date (the document's
/// date or the opening date, never the save time), the document it comes from (if any) and a note.
/// </summary>
public sealed record StockMovement
{
    public required StockType StockType { get; init; }

    public required StockChange Change { get; init; }

    public required DateOnly MovementDate { get; init; }

    public StockDocument? Document { get; init; }

    public string? Note { get; init; }
}
