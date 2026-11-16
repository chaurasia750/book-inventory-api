using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Price;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>R19 over the price store; the default version comes from the one rule of <see cref="DefaultPrice"/> (R4), shared with <see cref="PriceDirectory"/>.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPurchasePriceDirectory))]
public sealed class PurchasePriceDirectory(IPriceStore store) : IPurchasePriceDirectory
{
    public async Task<IReadOnlyDictionary<Guid, PurchasePriceReference>> PurchasePricesAsync(
        IReadOnlyCollection<Guid> itemIds, DateOnly onDate, CancellationToken cancellationToken = default)
    {
        var ids = itemIds.Distinct().ToList();
        if (ids.Count > IPurchasePriceDirectory.MaxItems)
            throw new ValidationFailureException(PriceMessages.TooManyItems);

        var byItem = (await store.VersionsOfItemsAsync(ids, cancellationToken)).ToLookup(v => v.ItemId);
        var prices = new Dictionary<Guid, PurchasePriceReference>();
        foreach (var id in ids)
        {
            var versions = byItem[id].ToList();
            var defaultId = DefaultPrice.IdOf(versions.Select(v => new VersionStamp(v.Id, v.EffectiveFrom, v.CreatedAt)), onDate);
            var version = versions.FirstOrDefault(v => v.Id == defaultId);
            if (version is not null)
                prices[id] = new PurchasePriceReference(id, version.Id, version.VersionName, version.PurchaseAmount);
        }

        return prices;
    }
}
