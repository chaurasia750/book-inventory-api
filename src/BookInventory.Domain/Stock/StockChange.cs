namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R4, R5, R15: what one movement does to one stock: its kind and a whole quantity in or out. Exactly one of the two
/// quantities is greater than zero, the other is zero; whether the kind allows it is checked by <see cref="ItemStock"/>.
/// </summary>
public sealed record StockChange(MovementKind Kind, int QuantityIn, int QuantityOut)
{
    public bool IsIn => QuantityIn > 0;

    /// <summary>The signed change of the quantity: in minus out.</summary>
    public int Net => QuantityIn - QuantityOut;

    public static StockChange In(MovementKind kind, int quantity) => new(kind, quantity, 0);

    public static StockChange Out(MovementKind kind, int quantity) => new(kind, 0, quantity);

    /// <summary>R11: the opposite of this change, posted when a document is cancelled.</summary>
    public StockChange Reversed() => new(MovementKind.Reversal, QuantityOut, QuantityIn);
}
