using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Companies;
using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// R6, R9, R10, R16: the stock list and one item's stock with its history. Items are found, searched, filtered and sorted through
/// the item directory (item R16, R17), only stock-managed ones; an item with no stock card has zero in every stock. The quantity
/// after each entry is computed from the entries themselves, oldest first.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class StockQueryService(
    IStockStore store, IItemDirectory items, StockManagedItems stockItems, IUserDirectory users, ICurrentCompany currentCompany)
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;
    private const int ScanPageSize = 200;

    public async Task<IPagedList<StockListRow>> ListAsync(StockListQuery query, CancellationToken cancellationToken)
    {
        var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
        var pageSize = Math.Clamp(query.PageSize < 1 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);
        var selection = new ItemSelection(query.VendorId, query.Category, query.ClassName, OnlyActive: !query.IncludeInactive, StockManaged: true);

        var (ids, total) = query.AtOrBelowZeroOnly
            ? await AtOrBelowZeroPageAsync(query, selection, pageIndex, pageSize, cancellationToken)
            : await PageAsync(new ItemPageRequest(query.Search, selection, ItemOrder(query.OrderBy), pageIndex, pageSize), cancellationToken);

        var references = await items.ReferencesAsync(ids, cancellationToken);
        var totals = (await store.TotalsAsync(ids, cancellationToken)).ToDictionary(t => t.ItemId);
        return new PagedList<StockListRow>
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            IndexFrom = 1,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = [.. ids.Where(references.ContainsKey).Select(id => RowOf(references[id], totals.GetValueOrDefault(id)))]
        };
    }

    public async Task<ItemStockResponse> ItemStockAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await stockItems.RequireAsync(itemId, cancellationToken);
        var totals = (await store.TotalsAsync([itemId], cancellationToken)).FirstOrDefault() ?? new StockTotals(itemId, 0, 0, 0);
        var entries = await store.EntriesOfItemAsync(itemId, cancellationToken);
        var names = await NamesAsync(cancellationToken);

        return new ItemStockResponse(
            item.Id, item.Code, item.Name, item.IsActive, totals.MainQuantity, totals.DamagedQuantity, totals.SampleQuantity, History(entries, names));
    }

    /// <summary>R4, R10: the entries newest first, each with the quantity of its stock type after it (running sum, oldest first).</summary>
    private static List<StockEntryResponse> History(IReadOnlyList<StockEntry> oldestFirst, IReadOnlyDictionary<Guid, string> names)
    {
        var running = new Dictionary<StockType, int>();
        var history = new List<StockEntryResponse>(oldestFirst.Count);
        foreach (var e in oldestFirst)
        {
            running[e.StockType] = running.GetValueOrDefault(e.StockType) + e.QuantityIn - e.QuantityOut;
            history.Add(new StockEntryResponse(
                e.Id, e.StockType, e.MovementKind, e.QuantityIn, e.QuantityOut, running[e.StockType], e.MovementDate,
                e.DocumentType, e.DocumentNumber, e.Note, e.CreatedAt, e.CreatedBy, names.GetValueOrDefault(e.CreatedBy)));
        }

        history.Reverse();
        return history;
    }

    private async Task<(IReadOnlyList<Guid> Ids, int Total)> PageAsync(ItemPageRequest request, CancellationToken cancellationToken)
    {
        var page = await items.PageAsync(request, cancellationToken);
        return (page.Ids, page.TotalCount);
    }

    /// <summary>
    /// R9: the "zero or below" filter needs the quantities, which the item directory does not know: the matching items are read in
    /// pages (at most <see cref="IItemDirectory.MaxBulkItems"/>), kept when Main is zero or below, and paged here.
    /// </summary>
    private async Task<(IReadOnlyList<Guid> Ids, int Total)> AtOrBelowZeroPageAsync(
        StockListQuery query, ItemSelection selection, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var matching = new List<Guid>();
        for (var scan = 1; matching.Count < IItemDirectory.MaxBulkItems; scan++)
        {
            var page = await items.PageAsync(new ItemPageRequest(query.Search, selection, ItemOrder(query.OrderBy), scan, ScanPageSize), cancellationToken);
            matching.AddRange(page.Ids);
            if (page.Ids.Count < ScanPageSize || matching.Count >= page.TotalCount)
                break;
        }

        var positive = (await store.TotalsAsync(matching, cancellationToken)).Where(t => t.MainQuantity > 0).Select(t => t.ItemId).ToHashSet();
        var low = matching.Where(id => !positive.Contains(id)).ToList();
        return ([.. low.Skip((pageIndex - 1) * pageSize).Take(pageSize)], low.Count);
    }

    /// <summary>R13: the names of the company's accounts, to show who made an entry; an entered Super Admin is shown by id only.</summary>
    private async Task<IReadOnlyDictionary<Guid, string>> NamesAsync(CancellationToken cancellationToken) =>
        currentCompany.CompanyId is { } companyId
            ? (await users.ListCompanyAccountsAsync(companyId, cancellationToken)).ToDictionary(u => u.Id, u => u.FullName)
            : new Dictionary<Guid, string>();

    private static StockListRow RowOf(ItemReference item, StockTotals? totals) =>
        new(
            item.Id, item.Code, item.Name, item.IsActive,
            totals?.MainQuantity ?? 0, totals?.DamagedQuantity ?? 0, totals?.SampleQuantity ?? 0,
            (totals?.MainQuantity ?? 0) <= 0);

    /// <summary>The stock list sorts by the item's name or code; anything else falls back to the name.</summary>
    private static string ItemOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = parts.Length > 0 && parts[0].Equals("ItemCode", StringComparison.OrdinalIgnoreCase) ? "Code" : "Name";
        var descending = parts.Length > 1 && parts[1].Equals("DESC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(descending ? "DESC" : "ASC")}";
    }
}
