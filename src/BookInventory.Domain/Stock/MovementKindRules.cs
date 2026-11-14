namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R5: the BRD movement table in one place: for each movement kind, the stock types it may touch and whether it
/// moves stock in, out or either way. Opening, adjustment and reversal may touch any stock type.
/// </summary>
public static class MovementKindRules
{
    private enum Direction
    {
        In,
        Out,
        Either
    }

    private static readonly StockType[] AnyType = [StockType.Main, StockType.Damaged, StockType.Sample];

    private static readonly IReadOnlyDictionary<MovementKind, (StockType[] Types, Direction Direction)> Table =
        new Dictionary<MovementKind, (StockType[], Direction)>
        {
            [MovementKind.Opening] = (AnyType, Direction.In),
            [MovementKind.Adjustment] = (AnyType, Direction.Either),
            [MovementKind.Purchase] = ([StockType.Main], Direction.In),
            [MovementKind.SchoolInvoice] = ([StockType.Main], Direction.Out),
            [MovementKind.CounterSale] = ([StockType.Main], Direction.Out),
            [MovementKind.SchoolReturnGood] = ([StockType.Main], Direction.In),
            [MovementKind.SchoolReturnDamaged] = ([StockType.Damaged], Direction.In),
            [MovementKind.VendorReturn] = ([StockType.Main, StockType.Damaged], Direction.Out),
            [MovementKind.SampleFromVendor] = ([StockType.Sample], Direction.In),
            [MovementKind.SampleToSchool] = ([StockType.Sample], Direction.Out),
            [MovementKind.SampleFromSchool] = ([StockType.Sample], Direction.In),
            [MovementKind.SampleToVendor] = ([StockType.Sample], Direction.Out),
            [MovementKind.Reversal] = (AnyType, Direction.Either)
        };

    /// <summary>Whether a change of this kind may move this stock type in the given direction (<paramref name="isIn"/>).</summary>
    public static bool Allows(MovementKind kind, StockType stockType, bool isIn)
    {
        if (!Table.TryGetValue(kind, out var rule) || !rule.Types.Contains(stockType))
            return false;

        return rule.Direction == Direction.Either || rule.Direction == (isIn ? Direction.In : Direction.Out);
    }
}
