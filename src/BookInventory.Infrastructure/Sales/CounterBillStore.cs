using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>The data side of the counter bills (spec sales R42-R49). Every query is already limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICounterBillStore))]
public sealed class CounterBillStore(IUnitOfWork unitOfWork) : ICounterBillStore
{
    private static readonly string[] SortableColumns = ["BillDate", "Number", "CreatedAt"];

    private readonly IRepository<CounterBill> _bills = unitOfWork.Repository<CounterBill>();

    public Task AddAsync(CounterBill bill, CancellationToken cancellationToken = default) => _bills.AddAsync(bill, cancellationToken);

    public Task<CounterBill?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _bills.Table.Include(b => b.Lines).Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<CounterBill?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _bills.TableNoTracking.Include(b => b.Lines).Include(b => b.Payments).FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task<IPagedList<CounterBill>> SearchAsync(CounterBillSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var className = query.ClassName?.Trim().ToLowerInvariant();
        var (counterId, schoolId, studentId, status, from, to) = (query.CounterId, query.SchoolId, query.StudentId, query.Status, query.From, query.To);

        return await _bills.TableNoTracking
            .Where(b => counterId == null || b.CounterId == counterId)
            .Where(b => schoolId == null || b.SchoolId == schoolId)
            .Where(b => studentId == null || b.StudentId == studentId)
            .Where(b => status == null || b.Status == status)
            .Where(b => from == null || b.BillDate >= from)
            .Where(b => to == null || b.BillDate <= to)
            .Where(b => string.IsNullOrEmpty(className) || b.StudentClass.ToLower() == className)
            .Where(b => string.IsNullOrEmpty(search) || b.Number.ToLower().Contains(search) || b.StudentName.ToLower().Contains(search))
            .OrderBy(WhitelistedOrder(query.OrderBy))
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
