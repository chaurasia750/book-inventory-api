using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// The persistence port of the school refunds (spec sales R64-R69). Every read is already limited to the session's company by the
/// platform filter (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IRefundStore
{
    Task AddAsync(SchoolRefund refund, CancellationToken cancellationToken = default);

    /// <summary>The refund about to be cancelled: tracked. <c>null</c> for an unknown id or another company's.</summary>
    Task<SchoolRefund?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<SchoolRefund?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R69: the paged, searchable, filterable list, newest first by default.</summary>
    Task<IPagedList<SchoolRefund>> SearchAsync(RefundSearchQuery query, CancellationToken cancellationToken = default);
}
