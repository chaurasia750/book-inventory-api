using BookInventory.BuildingBlocks.Domain.Models;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>One list row as the store reads it: the vendor is still an id (its name is added from the vendor directory).</summary>
public sealed record ItemRow(
    Guid Id,
    string Code,
    string Name,
    Domain.Item.ItemCategory Category,
    string? ClassName,
    Guid? VendorId,
    decimal GstRatePercent,
    bool IsStockManaged,
    Domain.Item.ItemStatus Status,
    DateTime CreatedAt);

/// <summary>
/// The persistence port of the <c>item</c> module. Every read is already limited to the session's company by the platform
/// filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IItemStore
{
    Task AddAsync(ItemRoot item, CancellationToken cancellationToken = default);

    /// <summary>The item about to be changed: tracked, with its barcodes. <c>null</c> for an unknown id or another company's.</summary>
    Task<ItemRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The item for reading: not tracked, with its barcodes.</summary>
    Task<ItemRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R6: whether one of these (normalised) barcodes is on another live item of the company; <paramref name="excludingItemId"/> is the item being changed.</summary>
    Task<bool> AnyBarcodeUsedAsync(IReadOnlyCollection<string> barcodes, Guid? excludingItemId, CancellationToken cancellationToken = default);

    /// <summary>R12: the paged, searchable, filterable list; <paramref name="vendorIds"/> are the vendors whose name matched the search text.</summary>
    Task<IPagedList<ItemRow>> SearchAsync(
        ItemSearchQuery query, IReadOnlyCollection<Guid> vendorIds, bool activeOnly, CancellationToken cancellationToken = default);
}
