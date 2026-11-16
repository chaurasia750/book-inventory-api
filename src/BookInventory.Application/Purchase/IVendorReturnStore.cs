using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Purchase;

namespace BookInventory.Application.Purchase;

/// <summary>
/// The persistence port of the vendor returns (spec purchase R1-R13). Every read is already limited to the session's company by the
/// platform filter (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IVendorReturnStore
{
    Task AddAsync(VendorReturn vendorReturn, CancellationToken cancellationToken = default);

    /// <summary>The vendor return about to be cancelled: tracked, with its lines. <c>null</c> for an unknown id or another company's.</summary>
    Task<VendorReturn?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<VendorReturn?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R12: the paged, searchable, filterable list, newest first by default.</summary>
    Task<IPagedList<VendorReturn>> SearchAsync(VendorReturnSearchQuery query, CancellationToken cancellationToken = default);
}
