using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using BookInventory.Domain.Item;
using Microsoft.EntityFrameworkCore;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Infrastructure.Item;

/// <summary>
/// The data side of the item use cases (spec item R6, R12). Every query is already limited to the session's company by the
/// platform filter, the barcodes of removed rows included (a soft-deleted barcode is not found). The vendor's name is not
/// joined here: the query service adds it from the vendor directory.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IItemStore))]
public sealed class ItemStore(IUnitOfWork unitOfWork) : IItemStore
{
    private static readonly string[] SortableColumns = ["Name", "Code", "Category", "CreatedAt"];

    private readonly IRepository<ItemRoot> _items = unitOfWork.Repository<ItemRoot>();

    public Task AddAsync(ItemRoot item, CancellationToken cancellationToken = default) =>
        _items.AddAsync(item, cancellationToken);

    public Task<ItemRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _items.IncludeMultiple(i => i.Barcodes).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<ItemRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default) =>
        _items.TableNoTracking.Include(i => i.Barcodes).FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<bool> AnyBarcodeUsedAsync(IReadOnlyCollection<string> barcodes, Guid? excludingItemId, CancellationToken cancellationToken = default) =>
        barcodes.Count == 0
            ? Task.FromResult(false)
            : _items.TableNoTracking.AnyAsync(
                i => (excludingItemId == null || i.Id != excludingItemId) && i.Barcodes.Any(b => barcodes.Contains(b.Barcode)),
                cancellationToken);

    public async Task<IPagedList<ItemRow>> SearchAsync(
        ItemSearchQuery query, IReadOnlyCollection<Guid> vendorIds, bool activeOnly, CancellationToken cancellationToken = default) =>
        await Filtered(query, vendorIds, activeOnly)
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(RowOf)
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);

    public async Task<IReadOnlyList<Guid>> IdsAsync(
        ItemSearchQuery query, IReadOnlyCollection<Guid> vendorIds, int limit, CancellationToken cancellationToken = default) =>
        await Filtered(query, vendorIds, activeOnly: false)
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(i => i.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemRow>> RowsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        ids.Count == 0
            ? []
            : await _items.TableNoTracking.Where(i => ids.Contains(i.Id)).Select(RowOf).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ItemKeyMatch>> MatchKeysAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        if (keys.Count == 0)
            return [];

        var byCode = await _items.TableNoTracking
            .Where(i => keys.Contains(i.Code.ToUpper()))
            .Select(i => new ItemKeyMatch(i.Code.ToUpper(), i.Id, true))
            .ToListAsync(cancellationToken);
        var byBarcode = await _items.TableNoTracking
            .SelectMany(i => i.Barcodes)
            .Where(b => keys.Contains(b.Barcode))
            .Select(b => new ItemKeyMatch(b.Barcode, b.ItemId, false))
            .ToListAsync(cancellationToken);
        return [.. byCode, .. byBarcode];
    }

    /// <summary>R12: the one place the list filters and the search are written; the list, the id lookups and the directory all use it.</summary>
    private IQueryable<ItemRoot> Filtered(ItemSearchQuery query, IReadOnlyCollection<Guid> vendorIds, bool activeOnly)
    {
        var search = query.Search?.Trim().ToLowerInvariant();
        var barcodeSearch = query.Search?.Trim().ToUpperInvariant();
        var status = activeOnly ? ItemStatus.Active : query.Status;
        var category = query.Category;
        var vendorId = query.VendorId;
        var className = query.ClassName?.Trim().ToLowerInvariant();
        var stockManaged = query.IsStockManaged;

        return _items.TableNoTracking
            .Where(i => status == null || i.Status == status)
            .Where(i => category == null || i.Category == category)
            .Where(i => vendorId == null || i.VendorId == vendorId)
            .Where(i => stockManaged == null || i.IsStockManaged == stockManaged)
            .Where(i => string.IsNullOrEmpty(className) || (i.ClassName != null && i.ClassName.ToLower() == className))
            .Where(i => string.IsNullOrEmpty(search)
                        || i.Name.ToLower().Contains(search)
                        || i.Code.ToLower().Contains(search)
                        || (i.ClassName != null && i.ClassName.ToLower().Contains(search))
                        || (i.Subject != null && i.Subject.ToLower().Contains(search))
                        || i.Barcodes.Any(b => b.Barcode.Contains(barcodeSearch!))
                        || (i.VendorId != null && vendorIds.Contains(i.VendorId.Value)));
    }

    private static readonly System.Linq.Expressions.Expression<Func<ItemRoot, ItemRow>> RowOf =
        i => new ItemRow(i.Id, i.Code, i.Name, i.Category, i.ClassName, i.VendorId, i.GstRatePercent, i.IsStockManaged, i.Status, i.CreatedAt, i.HsnCode);

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
