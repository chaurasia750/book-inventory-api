using System.ComponentModel.DataAnnotations;
using BookInventory.Application.Item;
using BookInventory.Domain.Item;
using BookInventory.Domain.Price;

namespace BookInventory.Application.Price;

/// <summary>R2: the values of a new version.</summary>
public sealed record AddPriceVersionRequest(
    [param: Required, StringLength(PriceVersion.NameMaxLength)] string VersionName,
    [param: Required] DateOnly? EffectiveFrom,
    [param: Required] decimal? MrpAmount,
    [param: Required] decimal? SaleAmount,
    [param: Required] decimal? PurchaseAmount)
{
    public PriceDetails ToDetails() => new(VersionName, EffectiveFrom!.Value, MrpAmount!.Value, SaleAmount!.Value, PurchaseAmount!.Value);
}

/// <summary>R3: the edit has the same fields as an add.</summary>
public sealed record EditPriceVersionRequest(
    [param: Required, StringLength(PriceVersion.NameMaxLength)] string VersionName,
    [param: Required] DateOnly? EffectiveFrom,
    [param: Required] decimal? MrpAmount,
    [param: Required] decimal? SaleAmount,
    [param: Required] decimal? PurchaseAmount)
{
    public PriceDetails ToDetails() => new(VersionName, EffectiveFrom!.Value, MrpAmount!.Value, SaleAmount!.Value, PurchaseAmount!.Value);
}

/// <summary>R8, R10: which items a copy or a bulk change means; empty means every item of the company.</summary>
public sealed record PriceSelection(Guid? VendorId = null, ItemCategory? Category = null, string? ClassName = null, bool OnlyActive = false)
{
    public ItemSelection ToItemSelection() => new(VendorId, Category, ClassName, OnlyActive);
}

/// <summary>R8: copy one version of the selected items to a new name and date.</summary>
public sealed record CopyPriceVersionRequest(
    [param: Required, StringLength(PriceVersion.NameMaxLength)] string FromVersionName,
    [param: Required, StringLength(PriceVersion.NameMaxLength)] string ToVersionName,
    [param: Required] DateOnly? EffectiveFrom,
    PriceSelection? Selection = null);

/// <summary>R8: how many items were copied and how many were skipped, and why.</summary>
public sealed record CopyPriceResult(int ItemsSelected, int Copied, int SkippedNoSourceVersion, int SkippedAlreadyHasTarget);

/// <summary>R9: the list filters and paging. <see cref="OrderBy"/> is <c>ItemName</c> or <c>ItemCode</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
public sealed record PriceListQuery
{
    /// <summary>Free text over item name, code, barcode, class, subject and the vendor's name.</summary>
    public string? Search { get; init; }

    public ItemCategory? Category { get; init; }

    public Guid? VendorId { get; init; }

    public string? ClassName { get; init; }

    public string? OrderBy { get; init; } = "ItemName ASC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R4, R5: one version as it is read; the purchase price is empty for Staff.</summary>
public sealed record PriceVersionResponse(
    Guid Id,
    Guid ItemId,
    string VersionName,
    DateOnly EffectiveFrom,
    decimal MrpAmount,
    decimal SaleAmount,
    decimal? PurchaseAmount,
    bool IsDefault);

/// <summary>R9: one row of the price list: the item and its default version, or none.</summary>
public sealed record PriceListRow(Guid ItemId, string ItemCode, string ItemName, PriceVersionResponse? DefaultVersion);
