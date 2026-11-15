using BookInventory.Domain.Planning;

namespace BookInventory.Application.Planning;

/// <summary>Spec planning R1: the persistence port of class book lists (company-filtered).</summary>
public interface IClassBookListStore
{
    Task AddAsync(ClassBookList list, CancellationToken cancellationToken = default);

    void Remove(ClassBookList list);

    /// <summary>One class's list of a school's session, tracked, with its lines, or <c>null</c>.</summary>
    Task<ClassBookList?> FindForManagementAsync(Guid schoolId, string session, string className, CancellationToken cancellationToken = default);

    /// <summary>One class's list without its lines, tracked, to be removed (the lines stay with the removed list), or <c>null</c>.</summary>
    Task<ClassBookList?> FindForRemovalAsync(Guid schoolId, string session, string className, CancellationToken cancellationToken = default);

    /// <summary>Every list of a school's session with its lines; <paramref name="tracked"/> when they will be changed.</summary>
    Task<IReadOnlyList<ClassBookList>> ListsOfAsync(Guid schoolId, string session, bool tracked, CancellationToken cancellationToken = default);
}
