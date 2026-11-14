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
        (await store.RowsAsync(ids, cancellationToken)).ToDictionary(r => r.Id, r => new ItemReference(r.Id, r.Code, r.Name, r.Status == ItemStatus.Active));

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

    private static ItemSearchQuery QueryOf(ItemSelection selection) =>
        new()
        {
            VendorId = selection.VendorId,
            Category = selection.Category,
            ClassName = selection.ClassName,
            Status = selection.OnlyActive ? ItemStatus.Active : null
        };

    private async Task<IReadOnlyCollection<Guid>> VendorIdsAsync(string? search, CancellationToken cancellationToken) =>
        string.IsNullOrWhiteSpace(search) ? [] : await vendors.IdsMatchingAsync(search, cancellationToken);
}
