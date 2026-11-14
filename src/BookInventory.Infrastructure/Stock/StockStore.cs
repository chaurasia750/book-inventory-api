using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Stock;

/// <summary>
/// The data side of the stock use cases (spec stock R6, R7, R10, R11). Entries are reached through their root, never through a
/// repository of their own. Every query is already limited to the session's company by the platform filter.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IStockStore))]
public sealed class StockStore(IUnitOfWork unitOfWork) : IStockStore
{
    private readonly IRepository<ItemStock> _stocks = unitOfWork.Repository<ItemStock>();

    public Task AddAsync(ItemStock stock, CancellationToken cancellationToken = default) =>
        _stocks.AddAsync(stock, cancellationToken);

    public async Task<IReadOnlyList<ItemStock>> FindForPostingAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0 ? [] : await _stocks.Table.Where(s => itemIds.Contains(s.ItemId)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockKey>> OpeningsOfAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0
            ? []
            : await Entries()
                .Where(e => itemIds.Contains(e.ItemId) && e.MovementKind == MovementKind.Opening)
                .Select(e => new StockKey(e.ItemId, e.StockType))
                .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockEntry>> DocumentEntriesAsync(string documentType, Guid documentId, CancellationToken cancellationToken = default) =>
        await Entries().Where(e => e.DocumentType == documentType && e.DocumentId == documentId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockTotals>> TotalsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0
            ? []
            : await _stocks.TableNoTracking
                .Where(s => itemIds.Contains(s.ItemId))
                .Select(s => new StockTotals(s.ItemId, s.MainQuantity, s.DamagedQuantity, s.SampleQuantity))
                .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StockEntry>> EntriesOfItemAsync(Guid itemId, CancellationToken cancellationToken = default) =>
        await Entries().Where(e => e.ItemId == itemId).OrderBy(e => e.CreatedAt).ThenBy(e => e.Id).ToListAsync(cancellationToken);

    private IQueryable<StockEntry> Entries() => _stocks.TableNoTracking.SelectMany(s => s.Entries);
}
