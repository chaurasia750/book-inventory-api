using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Price;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Price;

/// <summary>
/// The data side of the price use cases (spec price R2, R4, R8, R9). Every query is already limited to the session's company by
/// the platform filter.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPriceStore))]
public sealed class PriceStore(IUnitOfWork unitOfWork) : IPriceStore
{
    private readonly IRepository<PriceVersion> _versions = unitOfWork.Repository<PriceVersion>();

    public Task AddAsync(PriceVersion version, CancellationToken cancellationToken = default) =>
        _versions.AddAsync(version, cancellationToken);

    public Task AddRangeAsync(IEnumerable<PriceVersion> versions, CancellationToken cancellationToken = default) =>
        _versions.AddRangeAsync(versions, cancellationToken);

    public Task<PriceVersion?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _versions.Table.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

    public Task<bool> IsNameTakenAsync(Guid itemId, string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default) =>
        _versions.TableNoTracking.AnyAsync(
            v => v.ItemId == itemId && v.VersionNameNormalized == normalizedName && (excludingId == null || v.Id != excludingId), cancellationToken);

    public async Task<IReadOnlyList<PriceVersion>> VersionsOfItemsAsync(IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0
            ? []
            : await _versions.TableNoTracking.Where(v => itemIds.Contains(v.ItemId)).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PriceVersion>> FindNamedForManagementAsync(
        IReadOnlyCollection<Guid> itemIds, string normalizedName, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0
            ? []
            : await _versions.Table
                .Where(v => itemIds.Contains(v.ItemId) && v.VersionNameNormalized == normalizedName)
                .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PriceVersion>> VersionsNamedAsync(
        IReadOnlyCollection<Guid> itemIds, string normalizedName, CancellationToken cancellationToken = default) =>
        itemIds.Count == 0
            ? []
            : await _versions.TableNoTracking
                .Where(v => itemIds.Contains(v.ItemId) && v.VersionNameNormalized == normalizedName)
                .ToListAsync(cancellationToken);
}
