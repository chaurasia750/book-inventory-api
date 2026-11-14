using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>An item and one of its stock types.</summary>
public sealed record StockKey(Guid ItemId, StockType StockType);

/// <summary>The three running quantities of one item.</summary>
public sealed record StockTotals(Guid ItemId, int MainQuantity, int DamagedQuantity, int SampleQuantity);

/// <summary>
/// The persistence port of the <c>stock</c> module. Every read is already limited to the session's company by the platform filter
/// (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IStockStore
{
    Task AddAsync(ItemStock stock, CancellationToken cancellationToken = default);

    /// <summary>The stock cards of these items, tracked, without their history (new entries are appended, R6).</summary>
    Task<IReadOnlyList<ItemStock>> FindForPostingAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);

    /// <summary>R7: which of these items already have an opening, per stock type.</summary>
    Task<IReadOnlyList<StockKey>> OpeningsOfAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);

    /// <summary>R11: every entry a document posted (and any reversal of it), not tracked.</summary>
    Task<IReadOnlyList<StockEntry>> DocumentEntriesAsync(string documentType, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>R10: the quantities of these items; an item with no stock card is left out (its quantities are zero).</summary>
    Task<IReadOnlyList<StockTotals>> TotalsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);

    /// <summary>R10: the history of one item, oldest first, not tracked.</summary>
    Task<IReadOnlyList<StockEntry>> EntriesOfItemAsync(Guid itemId, CancellationToken cancellationToken = default);
}
