using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Item;

/// <summary>
/// Spec item R16: the read capability other modules use. It is built on the item store (so the list, the id lookups and the
/// directory share one filter, R12) and the vendor directory (so a search by vendor name needs no join). Nothing is tracked and
/// nothing is changed; the company filter of the platform limits every answer to the session's company.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IItemDirectory))]
public sealed class ItemDirectory(IItemStore store, IVendorDirectory vendors) : IItemDirectory
{
    public async Task<ItemReference?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        (await ReferencesAsync([id], cancellationToken)).GetValueOrDefault(id);

    public async Task<IReadOnlyDictionary<Guid, ItemReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        (await store.RowsAsync(ids, cancellationToken)).ToDictionary(r => r.Id, r => new ItemReference(r.Id, r.Code, r.Name, r.Status == ItemStatus.Active, r.IsStockManaged));

    public async Task<IReadOnlyDictionary<Guid, ItemSaleReference>> SaleReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default) =>
        (await store.RowsAsync(ids, cancellationToken)).ToDictionary(
            r => r.Id,
            r => new ItemSaleReference(new ItemReference(r.Id, r.Code, r.Name, r.Status == ItemStatus.Active, r.IsStockManaged), r.HsnCode, r.GstRatePercent));

    public async Task<ItemIdPage> PageAsync(ItemPageRequest request, CancellationToken cancellationToken = default)
    {
        var query = QueryOf(request.Selection) with
        {
            Search = request.Search,
            OrderBy = request.OrderBy,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
        var vendorIds = await VendorIdsAsync(request.Search, cancellationToken);

        var page = await store.SearchAsync(query, vendorIds, activeOnly: false, cancellationToken);
        return new ItemIdPage([.. page.Items.Select(r => r.Id)], page.TotalCount);
    }

    public Task<IReadOnlyList<Guid>> AllIdsAsync(ItemSelection selection, CancellationToken cancellationToken = default) =>
        store.IdsAsync(QueryOf(selection) with { OrderBy = "Code ASC" }, [], IItemDirectory.MaxBulkItems, cancellationToken);

    public async Task<IReadOnlyDictionary<string, ItemReference>> FindByCodeOrBarcodeAsync(
        IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default)
    {
        var normalized = keys.Select(k => k.Trim().ToUpperInvariant()).Where(k => k.Length > 0).Distinct().ToList();
        var matches = await store.MatchKeysAsync(normalized, cancellationToken);
        var references = await ReferencesAsync([.. matches.Select(m => m.ItemId).Distinct()], cancellationToken);

        return matches
            .Where(m => references.ContainsKey(m.ItemId))
            .GroupBy(m => m.Key)
            .ToDictionary(g => g.Key, g => references[g.OrderByDescending(m => m.IsCode).First().ItemId]);
    }

    private static ItemSearchQuery QueryOf(ItemSelection selection) =>
        new()
        {
            VendorId = selection.VendorId,
            Category = selection.Category,
            ClassName = selection.ClassName,
            Status = selection.OnlyActive ? ItemStatus.Active : null,
            IsStockManaged = selection.StockManaged
        };

    private async Task<IReadOnlyCollection<Guid>> VendorIdsAsync(string? search, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(search) ? [] : await vendors.IdsMatchingAsync(search, cancellationToken);
}
