using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Application.School;

/// <summary>
/// The persistence port of the <c>school</c> module. Every read is already limited to the session's company by the
/// platform filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request
/// transaction commits.
/// </summary>
public interface ISchoolStore
{
    Task AddAsync(SchoolRoot school, CancellationToken cancellationToken = default);

    /// <summary>The school about to be changed: tracked, with its classes. <c>null</c> for an unknown id or another company's.</summary>
    Task<SchoolRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The school for reading: not tracked, with its classes ordered.</summary>
    Task<SchoolRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// R15: the company's code counter, created on first use, taken under a transaction-scoped lock so two parallel
    /// creations in one company queue behind each other and get different numbers.
    /// </summary>
    Task<SchoolCodeCounter> LockCounterAsync(CancellationToken cancellationToken = default);

    /// <summary>R11: the paged, searchable, filterable list; <paramref name="activeOnly"/> is the Staff view (R9).</summary>
    Task<IPagedList<SchoolSummary>> SearchAsync(SchoolSearchQuery query, bool activeOnly, CancellationToken cancellationToken = default);
}
