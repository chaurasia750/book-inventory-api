using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Item;
using Microsoft.Extensions.DependencyInjection;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R9, R12: the item list and detail. The company comes from the platform filter; the only rule applied here is the Staff view:
/// Staff see active items only, and an inactive item is for them an unknown id. Vendor names come from the vendor directory
/// (vendor R15), never from a join.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ItemQueryService(IItemStore store, IVendorDirectory vendors, ICurrentUser currentUser)
{
    private const string StaffRole = "STAFF";

    public async Task<IPagedList<ItemSummary>> SearchAsync(ItemSearchQuery query, CancellationToken cancellationToken)
    {
        var matchingVendors = string.IsNullOrWhiteSpace(query.Search)
            ? []
            : await vendors.IdsMatchingAsync(query.Search, cancellationToken);
        var page = await store.SearchAsync(query, matchingVendors, activeOnly: IsStaff, cancellationToken);

        var names = await vendors.NamesAsync([.. page.Items.Where(r => r.VendorId is not null).Select(r => r.VendorId!.Value).Distinct()], cancellationToken);
        return new PagedList<ItemSummary>
        {
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            IndexFrom = page.IndexFrom,
            TotalCount = page.TotalCount,
            TotalPages = page.TotalPages,
            Items =
            [
                .. page.Items.Select(r => new ItemSummary(
                    r.Id, r.Code, r.Name, r.Category, r.ClassName, r.VendorId,
                    r.VendorId is { } v && names.TryGetValue(v, out var name) ? name : null,
                    r.GstRatePercent, r.IsStockManaged, r.Status, r.CreatedAt))
            ]
        };
    }

    public async Task<ItemDetailResponse> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var item = await store.FindForReadAsync(id, cancellationToken);
        if (item is null || (IsStaff && !item.IsActive))
            throw new NotFoundException(ItemMessages.NotFound);

        var vendor = item.VendorId is { } vendorId ? await vendors.GetAsync(vendorId, cancellationToken) : null;
        return ItemResponses.Detail(item, vendor?.Name);
    }

    private bool IsStaff => currentUser.Role == StaffRole;
}

/// <summary>The one projection of an item into its detail response, used by the create and the read use cases.</summary>
internal static class ItemResponses
{
    public static ItemDetailResponse Detail(ItemRoot item, string? vendorName) =>
        new(
            item.Id, item.Code, item.Name, item.Category, item.VendorId, vendorName, item.ClassName, item.Subject, item.HsnCode,
            item.GstRatePercent, item.IsStockManaged, item.Status, item.CreatedAt, item.UpdatedAt,
            [.. item.Barcodes.Select(b => b.Barcode)]);
}
