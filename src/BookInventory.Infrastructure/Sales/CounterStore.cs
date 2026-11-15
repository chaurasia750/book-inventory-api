using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>
/// The data side of the counters and the students of the school counter sale (spec sales R38-R41, R49). Every query is already
/// limited to the session's company by the platform filter.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICounterStore))]
public sealed class CounterStore(IUnitOfWork unitOfWork) : ICounterStore
{
    private static readonly string[] SortableColumns = ["CreatedAt", "Name", "StartDate"];

    private readonly IRepository<Counter> _counters = unitOfWork.Repository<Counter>();
    private readonly IRepository<Student> _students = unitOfWork.Repository<Student>();

    public Task AddCounterAsync(Counter counter, CancellationToken cancellationToken = default) => _counters.AddAsync(counter, cancellationToken);

    public Task<Counter?> FindCounterForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _counters.Table.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Counter?> FindCounterForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _counters.TableNoTracking.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IPagedList<Counter>> SearchCountersAsync(
        CounterSearchQuery query, IReadOnlyCollection<Guid> schoolIds, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var session = query.Session?.Trim();
        var schoolId = query.SchoolId;
        var status = query.Status;

        return await _counters.TableNoTracking
            .Where(c => schoolId == null || c.SchoolId == schoolId)
            .Where(c => status == null || c.Status == status)
            .Where(c => string.IsNullOrEmpty(session) || c.Session == session)
            .Where(c => string.IsNullOrEmpty(search) || c.Name.ToLower().Contains(search) || schoolIds.Contains(c.SchoolId))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);
    }

    public Task AddStudentAsync(Student student, CancellationToken cancellationToken = default) => _students.AddAsync(student, cancellationToken);

    public Task<Student?> FindStudentAsync(Guid schoolId, string nameNormalized, string className, CancellationToken cancellationToken = default) =>
        _students.Table.FirstOrDefaultAsync(
            s => s.SchoolId == schoolId && s.NameNormalized == nameNormalized && s.ClassName == className, cancellationToken);

    public Task<Student?> FindStudentByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _students.Table.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Student>> SuggestStudentsAsync(
        Guid schoolId, string normalizedText, string? className, CancellationToken cancellationToken = default) =>
        await _students.TableNoTracking
            .Where(s => s.SchoolId == schoolId && s.NameNormalized.Contains(normalizedText))
            .Where(s => className == null || s.ClassName == className)
            .OrderBy(s => s.NameNormalized)
            .ThenBy(s => s.ClassName)
            .Take(ICounterStore.MaxSuggestions)
            .ToListAsync(cancellationToken);

    /// <summary>The platform <c>OrderBy(string)</c> accepts any property, so the column is checked here first; anything else falls back to the default order.</summary>
    private static string? WhitelistedOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = SortableColumns.FirstOrDefault(c => parts.Length > 0 && c.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        if (column is null)
            return null;

        var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(descending ? "DESC" : "ASC")}";
    }
}
