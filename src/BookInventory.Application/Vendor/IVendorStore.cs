using BookInventory.BuildingBlocks.Domain.Models;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;

namespace BookInventory.Application.Vendor;

/// <summary>
/// The persistence port of the <c>vendor</c> module. Every read is already limited to the session's company by the platform
/// filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IVendorStore
{
    Task AddAsync(VendorRoot vendor, CancellationToken cancellationToken = default);

    /// <summary>The vendor about to be changed: tracked. <c>null</c> for an unknown id or another company's.</summary>
    Task<VendorRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The vendor for reading: not tracked.</summary>
    Task<VendorRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R4: whether another vendor of the company already has this normalised name; <paramref name="excludingId"/> is the vendor being edited.</summary>
    Task<bool> IsNameTakenAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default);

    /// <summary>R10: the paged, searchable, filterable list.</summary>
    Task<IPagedList<VendorSummary>> SearchAsync(VendorSearchQuery query, CancellationToken cancellationToken = default);
}
