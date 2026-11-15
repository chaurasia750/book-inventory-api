namespace BookInventory.Application.Stock;

/// <summary>
/// R20: the capability other modules (the sales module first, for its low-stock warning) use to read the Main, Damaged and Sample
/// quantities of items; an item with no stock card has zero. It answers only for the session's company and changes nothing.
/// </summary>
public interface IStockQuantities
{
    /// <summary>The cap of one call.</summary>
    const int MaxItems = 500;

    /// <summary>The quantities of each asked item (zero when it never moved); at most <see cref="MaxItems"/> ids.</summary>
    Task<IReadOnlyDictionary<Guid, StockTotals>> OfItemsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);
}
