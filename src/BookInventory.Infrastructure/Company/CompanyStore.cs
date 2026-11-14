using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Company;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Company;

/// <summary>
/// The data side of the company use cases (spec company R3, R7, R9, R10). Reads that are not about to change something
/// are not tracked; the list is paged (the platform caps the page size) and sorts only by a whitelisted column.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanyStore))]
public sealed class CompanyStore(IUnitOfWork unitOfWork) : ICompanyStore
{
    private static readonly string[] SortableColumns = ["Name", "CreatedAt"];

    private readonly IRepository<CompanyRoot> _companies = unitOfWork.Repository<CompanyRoot>();
    private readonly IRepository<CompanyEntry> _entries = unitOfWork.Repository<CompanyEntry>();

    public Task AddAsync(CompanyRoot company, CancellationToken cancellationToken = default) =>
        _companies.AddAsync(company, cancellationToken);

    public Task AddAsync(CompanyEntry entry, CancellationToken cancellationToken = default) =>
        _entries.AddAsync(entry, cancellationToken);

    public Task<CompanyRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _companies.IncludeMultiple(c => c.StatusHistory).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<CompanyRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _companies.TableNoTracking.Include(c => c.StatusHistory).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> IsNameTakenAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default) =>
        _companies.TableNoTracking.AnyAsync(
            c => c.NameNormalized == normalizedName && (excludingId == null || c.Id != excludingId), cancellationToken);

    public Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default) =>
        _companies.TableNoTracking.AnyAsync(c => c.Id == id && c.Status == CompanyStatus.Active, cancellationToken);

    public async Task<IPagedList<CompanySummary>> SearchAsync(CompanySearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = CompanyRoot.NormalizeName(query.Search);
        var status = query.Status;
        var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;

        return await _companies.TableNoTracking
            .Where(c => status == null || c.Status == status)
            .Where(c => search == string.Empty || c.NameNormalized.Contains(search))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(c => new CompanySummary(c.Id, c.Name, c.State, c.Status, c.CreatedAt))
            .ToPagedListAsync(pageIndex, query.PageSize, cancellationToken);
    }

    /// <summary>The platform <c>OrderBy(string)</c> accepts any property, so the column is checked here first; anything else falls back to the default order.</summary>
    private static string? WhitelistedOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = SortableColumns.FirstOrDefault(c => parts.Length > 0 && c.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        if (column is null)
            return null;

        var ascending = parts.Length > 1 && parts[1].Equals("ASC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(ascending ? "ASC" : "DESC")}";
    }
}
