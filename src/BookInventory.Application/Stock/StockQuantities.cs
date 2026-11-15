using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>R20 over the stock store's running totals.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IStockQuantities))]
public sealed class StockQuantities(IStockStore store) : IStockQuantities
{
    public async Task<IReadOnlyDictionary<Guid, StockTotals>> OfItemsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default)
    {
        var ids = itemIds.Distinct().Take(IStockQuantities.MaxItems).ToList();
        var totals = (await store.TotalsAsync(ids, cancellationToken)).ToDictionary(t => t.ItemId);
        return ids.ToDictionary(id => id, id => totals.GetValueOrDefault(id) ?? new StockTotals(id, 0, 0, 0));
    }
}
