using BookInventory.Application.Customer;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Infrastructure.Customer;

/// <summary>
/// The data side of the customer use cases (spec customer R3, R8, R11). Every query is already limited to the session's
/// company by the platform filter.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICustomerStore))]
public sealed class CustomerStore(IUnitOfWork unitOfWork) : ICustomerStore
{
    private static readonly string[] SortableColumns = ["Name", "Code", "CreatedAt"];

    private readonly IRepository<CustomerRoot> _customers = unitOfWork.Repository<CustomerRoot>();

    public Task AddAsync(CustomerRoot customer, CancellationToken cancellationToken = default) =>
        _customers.AddAsync(customer, cancellationToken);

    public Task<CustomerRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _customers.Table.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<CustomerRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _customers.TableNoTracking.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<bool> IsMobileTakenAsync(string mobile, Guid? excludingId, CancellationToken cancellationToken = default) =>
        _customers.TableNoTracking.AnyAsync(
            c => c.Mobile == mobile && (excludingId == null || c.Id != excludingId), cancellationToken);

    public async Task<IPagedList<CustomerRow>> SearchAsync(CustomerSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var status = query.Status;
        var schoolId = query.SchoolId;

        return await _customers.TableNoTracking
            .Where(c => status == null || c.Status == status)
            .Where(c => schoolId == null || c.SchoolId == schoolId)
            .Where(c => string.IsNullOrEmpty(search)
                        || c.NameNormalized.Contains(search)
                        || c.Code.ToLower().Contains(search)
                        || c.Mobile.Contains(search))
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(c => new CustomerRow(c.Id, c.Code, c.Name, c.Mobile, c.SchoolId, c.ClassName, c.Status, c.CreatedAt))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<CustomerRow>> SuggestAsync(string text, CancellationToken cancellationToken = default)
    {
        var name = string.Join(' ', text.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
        var digits = new string([.. text.Where(char.IsDigit)]);

        return await _customers.TableNoTracking
            .Where(c => c.Status == Domain.Customer.CustomerStatus.Active)
            .Where(c => c.NameNormalized.Contains(name) || (digits != string.Empty && c.Mobile.Contains(digits)))
            .OrderBy(c => c.NameNormalized)
            .Take(ICustomerStore.MaxSuggestions)
            .Select(c => new CustomerRow(c.Id, c.Code, c.Name, c.Mobile, c.SchoolId, c.ClassName, c.Status, c.CreatedAt))
            .ToListAsync(cancellationToken);
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
