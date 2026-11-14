using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// R1, R2, D4: the one place the stock module asks the item directory (item R16, R17) whether an item exists in the company and is
/// stock managed. The flag is never copied into this module.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StockManagedItems(IItemDirectory items)
{
    /// <summary>The item, or <c>404 stock.item_not_found</c> (also for another company's) or <c>400 stock.not_stock_managed</c>.</summary>
    public async Task<ItemReference> RequireAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await items.GetAsync(itemId, cancellationToken) ?? throw new NotFoundException(StockMessages.ItemNotFound);
        if (!item.IsStockManaged)
            throw new ValidationFailureException(StockMessages.NotStockManaged);

        return item;
    }

    /// <summary>R2, R11: which of these items are stock managed; unknown items are left out.</summary>
    public async Task<IReadOnlySet<Guid>> StockManagedOfAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken) =>
        (await items.ReferencesAsync(itemIds, cancellationToken)).Values.Where(r => r.IsStockManaged).Select(r => r.Id).ToHashSet();
}
