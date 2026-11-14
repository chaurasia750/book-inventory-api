using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// D1: the stock cards of items about to move, loaded tracked; an item that never moved gets a new card here, its first movement.
/// Shared by the opening, the adjustment, the posting capability and the import, so a card is started in one place only.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StockCards(IStockStore store)
{
    public async Task<IReadOnlyDictionary<Guid, ItemStock>> ForItemsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken)
    {
        var cards = (await store.FindForPostingAsync(itemIds, cancellationToken)).ToDictionary(s => s.ItemId);
        foreach (var itemId in itemIds.Where(id => !cards.ContainsKey(id)).Distinct().ToList())
        {
            var card = ItemStock.StartFor(itemId);
            await store.AddAsync(card, cancellationToken);
            cards[itemId] = card;
        }

        return cards;
    }

    public async Task<ItemStock> ForItemAsync(Guid itemId, CancellationToken cancellationToken) =>
        (await ForItemsAsync([itemId], cancellationToken))[itemId];
}
