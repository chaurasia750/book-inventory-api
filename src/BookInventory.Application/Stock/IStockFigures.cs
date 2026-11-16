namespace BookInventory.Application.Stock;

/// <summary>R10: the stock counts of the dashboard: active stock-managed items at or below zero Main stock, those below zero, and the Damaged pieces.</summary>
public sealed record StockCounts(int AtOrBelowZero, int BelowZero, int DamagedPieces);

/// <summary>R10: one item with its Main quantity above zero (to be valued by the caller).</summary>
public sealed record MainQuantity(Guid ItemId, int Quantity);

/// <summary>R10: one page of <see cref="MainQuantity"/> rows and how many there are in all.</summary>
public sealed record MainQuantityPage(IReadOnlyList<MainQuantity> Items, int TotalCount);

/// <summary>
/// R10: the capability the report module uses to read the stock figures of the dashboard, never by joining the stock tables. It
/// answers only for the session's company, returns plain values and changes nothing. Only active stock-managed items count; an
/// item without a stock card has zero (stock R20).
/// </summary>
public interface IStockFigures
{
    /// <summary>The cap of a page of <see cref="MainQuantitiesAsync"/>.</summary>
    const int MaxPageSize = 500;

    Task<StockCounts> CountsAsync(CancellationToken cancellationToken = default);

    /// <summary>The Main quantity of every active stock-managed item that has more than zero, ordered by item id, <paramref name="pageSize"/> (at most 500) per page.</summary>
    Task<MainQuantityPage> MainQuantitiesAsync(int pageIndex, int pageSize, CancellationToken cancellationToken = default);
}
