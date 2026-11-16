using BookInventory.Application.Purchase;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Purchase;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Purchase;

/// <summary>The data side of the vendor returns (spec purchase R1-R13). Every query is already limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IVendorReturnStore))]
public sealed class VendorReturnStore(IUnitOfWork unitOfWork) : IVendorReturnStore
{
    private static readonly string[] SortableColumns = ["ReturnDate", "Number", "CreatedAt"];

    private readonly IRepository<VendorReturn> _returns = unitOfWork.Repository<VendorReturn>();

    public Task AddAsync(VendorReturn vendorReturn, CancellationToken cancellationToken = default) => _returns.AddAsync(vendorReturn, cancellationToken);

    public Task<VendorReturn?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _returns.Table.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<VendorReturn?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _returns.TableNoTracking.Include(r => r.Lines).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<IPagedList<VendorReturn>> SearchAsync(VendorReturnSearchQuery query, CancellationToken cancellationToken = default)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var (vendorId, status, from, to) = (query.VendorId, query.Status, query.From, query.To);

        return await _returns.TableNoTracking
            .Where(r => vendorId == null || r.VendorId == vendorId)
            .Where(r => status == null || r.Status == status)
            .Where(r => from == null || r.ReturnDate >= from)
            .Where(r => to == null || r.ReturnDate <= to)
            .Where(r => string.IsNullOrEmpty(search) || r.Number.ToLower().Contains(search) || r.VendorName.ToLower().Contains(search) || r.VendorCode.ToLower().Contains(search))
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
