using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>The data side of the school refunds (spec sales R64-R69). Every query is already limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IRefundStore))]
public sealed class RefundStore(IUnitOfWork unitOfWork) : IRefundStore
{
    private static readonly string[] SortableColumns = ["RefundDate", "Number", "CreatedAt"];

    private readonly IRepository<SchoolRefund> _refunds = unitOfWork.Repository<SchoolRefund>();

    public Task AddAsync(SchoolRefund refund, CancellationToken cancellationToken = default) => _refunds.AddAsync(refund, cancellationToken);

    public Task<SchoolRefund?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _refunds.Table.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<SchoolRefund?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _refunds.TableNoTracking.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IPagedList<SchoolRefund>> SearchAsync(RefundSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var (schoolId, creditNoteId, status, from, to) = (query.SchoolId, query.CreditNoteId, query.Status, query.From, query.To);

        return await _refunds.TableNoTracking
            .Where(r => schoolId == null || r.SchoolId == schoolId)
            .Where(r => creditNoteId == null || r.CreditNoteId == creditNoteId)
            .Where(r => status == null || r.Status == status)
            .Where(r => from == null || r.RefundDate >= from)
            .Where(r => to == null || r.RefundDate <= to)
            .Where(r => string.IsNullOrEmpty(search) || r.Number.ToLower().Contains(search) || r.SchoolName.ToLower().Contains(search) || r.SchoolCode.ToLower().Contains(search))
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
