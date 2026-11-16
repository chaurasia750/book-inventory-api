using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// The persistence port of the counter bills (spec sales R42-R49). Every read is already limited to the session's company by the
/// platform filter (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ICounterBillStore
{
    Task AddAsync(CounterBill bill, CancellationToken cancellationToken = default);

    /// <summary>The bill about to be cancelled: tracked, with its lines and payments. <c>null</c> for an unknown id or another company's.</summary>
    Task<CounterBill?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CounterBill?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R50: the live bills of a date (and of one counter, when given) added up.</summary>
    Task<CounterDaySummary> DaySummaryAsync(CounterDaySummaryQuery query, CancellationToken cancellationToken = default);

    /// <summary>R49: the paged, searchable, filterable list, newest first by default.</summary>
    Task<IPagedList<CounterBill>> SearchAsync(CounterBillSearchQuery query, CancellationToken cancellationToken = default);
}
