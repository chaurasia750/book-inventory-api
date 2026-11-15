using BookInventory.Domain.Price;

namespace BookInventory.Application.Price;

/// <summary>
/// The persistence port of the <c>price</c> module. Every read is already limited to the session's company by the platform
/// filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IPriceStore
{
    Task AddAsync(PriceVersion version, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<PriceVersion> versions, CancellationToken cancellationToken = default);

    /// <summary>The version about to be changed: tracked. <c>null</c> for an unknown id or another company's.</summary>
    Task<PriceVersion?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R2: whether the item already has a version with this normalised name; <paramref name="excludingId"/> is the version being edited.</summary>
    Task<bool> IsNameTakenAsync(Guid itemId, string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default);

    /// <summary>R4, R9: all versions of the given items, not tracked.</summary>
    Task<IReadOnlyList<PriceVersion>> VersionsOfItemsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);

    /// <summary>R18: the versions with these ids, not tracked.</summary>
    Task<IReadOnlyList<PriceVersion>> VersionsByIdAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>R13: the versions of the given items with this normalised name, tracked, so a bulk change can edit them.</summary>
    Task<IReadOnlyList<PriceVersion>> FindNamedForManagementAsync(IReadOnlyCollection<Guid> itemIds, string normalizedName, CancellationToken cancellationToken = default);

    /// <summary>R8: the versions of the given items with this normalised name, not tracked.</summary>
    Task<IReadOnlyList<PriceVersion>> VersionsNamedAsync(IReadOnlyCollection<Guid> itemIds, string normalizedName, CancellationToken cancellationToken = default);
}
