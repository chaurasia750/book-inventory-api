using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// The persistence port of the counters and the students of the school counter sale. Every read is already limited to the session's
/// company by the platform filter (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ICounterStore
{
    /// <summary>The cap of <see cref="SuggestStudentsAsync"/> (R41).</summary>
    const int MaxSuggestions = 20;

    Task AddCounterAsync(Counter counter, CancellationToken cancellationToken = default);

    /// <summary>The counter about to be changed: tracked. <c>null</c> for an unknown id or another company's.</summary>
    Task<Counter?> FindCounterForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Counter?> FindCounterForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R49: the paged, searchable, filterable list; <paramref name="schoolIds"/> are the schools matched by the search text.</summary>
    Task<IPagedList<Counter>> SearchCountersAsync(CounterSearchQuery query, IReadOnlyCollection<Guid> schoolIds, CancellationToken cancellationToken = default);

    Task AddStudentAsync(Student student, CancellationToken cancellationToken = default);

    /// <summary>R40: the school's student with this normalised name and class, tracked, or <c>null</c>.</summary>
    Task<Student?> FindStudentAsync(Guid schoolId, string nameNormalized, string className, CancellationToken cancellationToken = default);

    Task<Student?> FindStudentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R41: at most <see cref="MaxSuggestions"/> students of the school whose name contains the text, optionally of one class.</summary>
    Task<IReadOnlyList<Student>> SuggestStudentsAsync(Guid schoolId, string normalizedText, string? className, CancellationToken cancellationToken = default);
}
