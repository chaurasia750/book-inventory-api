using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Price;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>R18 over the price store; the default version comes from the one rule of <see cref="DefaultPrice"/> (R4).</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPriceDirectory))]
public sealed class PriceDirectory(IPriceStore store) : IPriceDirectory
{
    public async Task<IReadOnlyDictionary<Guid, ItemPriceVersions>> VersionsOfItemsAsync(
        IReadOnlyCollection<Guid> itemIds, DateOnly onDate, CancellationToken cancellationToken = default)
    {
        var byItem = (await store.VersionsOfItemsAsync(itemIds, cancellationToken)).ToLookup(v => v.ItemId);
        return itemIds.Distinct().ToDictionary(id => id, id =>
        {
            var versions = byItem[id].ToList();
            var defaultId = DefaultPrice.IdOf(versions.Select(v => new VersionStamp(v.Id, v.EffectiveFrom, v.CreatedAt)), onDate);
            return new ItemPriceVersions(
                id,
                [.. versions.OrderByDescending(v => v.EffectiveFrom).ThenByDescending(v => v.CreatedAt).Select(ReferenceOf)],
                defaultId);
        });
    }

    public async Task<IReadOnlyDictionary<Guid, PriceVersionReference>> VersionsByIdAsync(
        IReadOnlyCollection<Guid> versionIds, CancellationToken cancellationToken = default) =>
        (await store.VersionsByIdAsync(versionIds, cancellationToken)).ToDictionary(v => v.Id, ReferenceOf);

    private static PriceVersionReference ReferenceOf(PriceVersion v) =>
        new(v.Id, v.ItemId, v.VersionName, v.EffectiveFrom, v.MrpAmount, v.SaleAmount);
}
