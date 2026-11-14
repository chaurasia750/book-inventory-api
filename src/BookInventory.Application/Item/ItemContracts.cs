using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Item;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>R2-R7: a new item. The code is generated (R10), so there is none here.</summary>
public sealed record CreateItemRequest(
    [param: Required, StringLength(ItemRoot.NameMaxLength)] string Name,
    [param: Required] ItemCategory? Category,
    Guid? VendorId = null,
    string? ClassName = null,
    string? Subject = null,
    string? HsnCode = null,
    decimal? GstRatePercent = null,
    bool? IsStockManaged = null,
    IReadOnlyList<string>? Barcodes = null)
{
    public ItemDetails ToDetails() => new(Name, Category!.Value, VendorId, ClassName, Subject, HsnCode, GstRatePercent, IsStockManaged);
}

/// <summary>R2-R5, R7: the master data an Owner may change; not the code, the status or the barcodes.</summary>
public sealed record EditItemRequest(
    [param: Required, StringLength(ItemRoot.NameMaxLength)] string Name,
    [param: Required] ItemCategory? Category,
    bool IsStockManaged,
    decimal GstRatePercent,
    Guid? VendorId = null,
    string? ClassName = null,
    string? Subject = null,
    string? HsnCode = null)
{
    public ItemDetails ToDetails() => new(Name, Category!.Value, VendorId, ClassName, Subject, HsnCode, GstRatePercent, IsStockManaged);
}

/// <summary>R6: the whole barcode list.</summary>
public sealed record ReplaceBarcodesRequest([param: Required] IReadOnlyList<string> Barcodes);

/// <summary>R12: the list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record ItemSearchQuery
{
    /// <summary>Free text over name, code, barcode, class, subject and the vendor's name.</summary>
    public string? Search { get; init; }

    public ItemCategory? Category { get; init; }

    public Guid? VendorId { get; init; }

    public string? ClassName { get; init; }

    public bool? IsStockManaged { get; init; }

    public ItemStatus? Status { get; init; }

    /// <summary>Whitelisted sort: <c>Name</c>, <c>Code</c>, <c>Category</c> or <c>CreatedAt</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "Name ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R12: one row of the item list; the vendor's name comes from the vendor directory.</summary>
public sealed record ItemSummary(
    Guid Id,
    string Code,
    string Name,
    ItemCategory Category,
    string? ClassName,
    Guid? VendorId,
    string? VendorName,
    decimal GstRatePercent,
    bool IsStockManaged,
    ItemStatus Status,
    DateTime CreatedAt);

/// <summary>The item page: master data, vendor, barcodes and status.</summary>
public sealed record ItemDetailResponse(
    Guid Id,
    string Code,
    string Name,
    ItemCategory Category,
    Guid? VendorId,
    string? VendorName,
    string? ClassName,
    string? Subject,
    string? HsnCode,
    decimal GstRatePercent,
    bool IsStockManaged,
    ItemStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<string> Barcodes);
