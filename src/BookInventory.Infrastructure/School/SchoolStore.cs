using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Iam.Companies;
using BookInventory.Application.School;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Infrastructure.School;

/// <summary>
/// The data side of the school use cases (spec school R9, R11, R15). Every query is already limited to the session's
/// company by the platform filter. The code counter is taken under a PostgreSQL advisory lock keyed on the company, held
/// until the request transaction ends, so parallel creations in one company are serialised (R15).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISchoolStore))]
public sealed class SchoolStore(IUnitOfWork unitOfWork, ICurrentCompany currentCompany) : ISchoolStore
{
    /// <summary>Arbitrary constant namespace of the advisory lock, combined with the company's hash so companies do not block each other.</summary>
    private const int CounterLockNamespace = 5_200_001;

    private static readonly string[] SortableColumns = ["Name", "Code", "CreatedAt"];

    private readonly IRepository<SchoolRoot> _schools = unitOfWork.Repository<SchoolRoot>();
    private readonly IRepository<SchoolCodeCounter> _counters = unitOfWork.Repository<SchoolCodeCounter>();

    public Task AddAsync(SchoolRoot school, CancellationToken cancellationToken = default) =>
        _schools.AddAsync(school, cancellationToken);

    public Task<SchoolRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _schools.IncludeMultiple(s => s.Classes).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<SchoolRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _schools.TableNoTracking.Include(s => s.Classes).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<SchoolCodeCounter> LockCounterAsync(CancellationToken cancellationToken = default)
    {
        var companyId = currentCompany.CompanyId ?? Guid.Empty;
        await _counters.ExecuteSqlAsync($"select pg_advisory_xact_lock({CounterLockNamespace}, {companyId.GetHashCode()})", cancellationToken);

        var counter = await _counters.Table.FirstOrDefaultAsync(cancellationToken);
        if (counter is not null)
            return counter;

        counter = SchoolCodeCounter.Start();
        await _counters.AddAsync(counter, cancellationToken);
        return counter;
    }

    public async Task<IPagedList<SchoolSummary>> SearchAsync(SchoolSearchQuery query, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var status = activeOnly ? SchoolStatus.Active : query.Status;
        var saleType = query.SaleType;
        var sessionType = query.SessionType;
        var state = query.State?.Trim();

        return await _schools.TableNoTracking
            .Where(s => status == null || s.Status == status)
            .Where(s => saleType == null || s.SaleType == saleType)
            .Where(s => sessionType == null || s.SessionType == sessionType)
            .Where(s => string.IsNullOrEmpty(state) || s.State == state)
            .Where(s => string.IsNullOrEmpty(search)
                        || s.Name.ToLower().Contains(search)
                        || s.Code.ToLower().Contains(search)
                        || (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(search))
                        || (s.Mobile != null && s.Mobile.Contains(search)))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(s => new SchoolSummary(
                s.Id, s.Code, s.Name, s.ContactPerson, s.Mobile, s.State, s.SessionType, s.SaleType, s.Status, s.CreatedAt))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);
    }

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
