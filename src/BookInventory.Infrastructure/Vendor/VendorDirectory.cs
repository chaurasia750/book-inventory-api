using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;

namespace BookInventory.Infrastructure.Vendor;

/// <summary>
/// Spec vendor R15: the read capability other modules use, over the same company-filtered repository as the vendor store, so it
/// only ever sees the session's company. Nothing is tracked and nothing is changed.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IVendorDirectory))]
public sealed class VendorDirectory(IUnitOfWork unitOfWork) : IVendorDirectory
{
    private readonly IRepository<VendorRoot> _vendors = unitOfWork.Repository<VendorRoot>();

    public async Task<VendorReference?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await ReferencesAsync([id], cancellationToken)).GetValueOrDefault(id);

    public async Task<IReadOnlyDictionary<Guid, VendorReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, VendorReference>();

        return await _vendors.TableNoTracking
            .Where(v => ids.Contains(v.Id))
            .Select(v => new VendorReference(v.Id, v.Code, v.Name, v.Status == Domain.Vendor.VendorStatus.Active))
            .ToDictionaryAsync(v => v.Id, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, string>> NamesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();

        return await _vendors.TableNoTracking
            .Where(v => ids.Contains(v.Id))
            .Select(v => new { v.Id, v.Name })
            .ToDictionaryAsync(v => v.Id, v => v.Name, cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> IdsMatchingAsync(string text, CancellationToken cancellationToken = default)
    {
        var search = text.Trim().ToLowerInvariant();
        if (search.Length == 0)
            return [];

        return await _vendors.TableNoTracking
            .Where(v => v.Name.ToLower().Contains(search) || v.Code.ToLower().Contains(search))
            .OrderBy(v => v.Name)
            .Select(v => v.Id)
            .Take(IVendorDirectory.MaxMatches)
            .ToListAsync(cancellationToken);
    }
}
