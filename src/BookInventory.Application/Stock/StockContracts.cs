using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Item;
using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>R7: the opening quantity of one stock type and the date counting starts.</summary>
public sealed record EnterOpeningRequest(
    [param: Required] StockType? StockType,
    [param: Required, Range(1, ItemStock.MaxQuantity)] int? Quantity,
    [param: Required] DateOnly? OpeningDate);

/// <summary>R8: the counted quantity of one stock type after a physical count, and why it differs.</summary>
public sealed record AdjustStockRequest(
    [param: Required] StockType? StockType,
    [param: Required, Range(0, ItemStock.MaxQuantity)] int? CountedQuantity,
    [param: Required, StringLength(ItemStock.ReasonMaxLength, MinimumLength = ItemStock.ReasonMinLength)] string Reason);

/// <summary>R7, R8: the entry just posted and the quantity of its stock type after it.</summary>
public sealed record StockPostedResponse(
    Guid EntryId,
    Guid ItemId,
    StockType StockType,
    MovementKind MovementKind,
    int QuantityIn,
    int QuantityOut,
    int Quantity);

/// <summary>R10: the list filters and paging. <see cref="OrderBy"/> is <c>ItemName</c> or <c>ItemCode</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
public sealed record StockListQuery
{
    /// <summary>Free text over item name, code, barcode, class, subject and the vendor's name.</summary>
    public string? Search { get; init; }

    public ItemCategory? Category { get; init; }

    public Guid? VendorId { get; init; }

    public string? ClassName { get; init; }

    /// <summary>R9: only items whose Main quantity is zero or below.</summary>
    public bool AtOrBelowZeroOnly { get; init; }

    /// <summary>R16: deactivated items are listed only when asked.</summary>
    public bool IncludeInactive { get; init; }

    public string? OrderBy { get; init; } = "ItemName ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R9, R10: one row of the stock list: the item, its three quantities and the warning mark (Main at zero or below).</summary>
public sealed record StockListRow(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    bool IsActive,
    int MainQuantity,
    int DamagedQuantity,
    int SampleQuantity,
    bool IsAtOrBelowZero);

/// <summary>R4, R10, R13: one entry of an item's history, with the quantity of its stock type after it and who made it and when.</summary>
public sealed record StockEntryResponse(
    Guid Id,
    StockType StockType,
    MovementKind MovementKind,
    int QuantityIn,
    int QuantityOut,
    int QuantityAfter,
    DateOnly MovementDate,
    string? DocumentType,
    string? DocumentNumber,
    string? Note,
    DateTime CreatedAt,
    Guid CreatedBy,
    string? CreatedByName);

/// <summary>R10: one item's three quantities and its history, newest first.</summary>
public sealed record ItemStockResponse(
    Guid ItemId,
    string ItemCode,
    string ItemName,
    bool IsActive,
    int MainQuantity,
    int DamagedQuantity,
    int SampleQuantity,
    IReadOnlyList<StockEntryResponse> Entries);
