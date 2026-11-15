namespace BookInventory.Application.Price;

/// <summary>R18: one price version as other modules see it: never the purchase price.</summary>
public sealed record PriceVersionReference(Guid Id, Guid ItemId, string VersionName, DateOnly EffectiveFrom, decimal MrpAmount, decimal SaleAmount);

/// <summary>R18: an item's versions, newest first, and the id of its default version on the asked date (R4), if any.</summary>
public sealed record ItemPriceVersions(Guid ItemId, IReadOnlyList<PriceVersionReference> Versions, Guid? DefaultVersionId);

/// <summary>
/// R18: the capability other modules (the sales module first) use to read prices, never by joining the price tables. It answers
/// only for the session's company, returns plain values without the purchase price, and changes nothing.
/// </summary>
public interface IPriceDirectory
{
    /// <summary>The versions of each item and its default on <paramref name="onDate"/>; an item without versions has an empty list.</summary>
    Task<IReadOnlyDictionary<Guid, ItemPriceVersions>> VersionsOfItemsAsync(IReadOnlyCollection<Guid> itemIds, DateOnly onDate, CancellationToken cancellationToken = default);

    /// <summary>The versions with these ids; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, PriceVersionReference>> VersionsByIdAsync(IReadOnlyCollection<Guid> versionIds, CancellationToken cancellationToken = default);
}
