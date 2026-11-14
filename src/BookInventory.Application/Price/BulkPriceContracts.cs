using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Price;

namespace BookInventory.Application.Price;

/// <summary>R11: one change of one price field: increase or decrease, by a percentage or an amount.</summary>
public sealed record PriceChangeRequest(
    [param: Required] PriceChangeKind? Kind,
    [param: Required] PriceChangeDirection? Direction,
    [param: Required] decimal? Value)
{
    public PriceChange ToChange() => new(Kind!.Value, Direction!.Value, Value!.Value);
}

/// <summary>R10, R11: which version of which items to change, and how each price field changes (a field left out stays as it is).</summary>
public sealed record BulkPriceChangeRequest(
    [param: Required, StringLength(PriceVersion.NameMaxLength)] string VersionName,
    PriceSelection? Selection = null,
    PriceChangeRequest? Mrp = null,
    PriceChangeRequest? Sale = null,
    PriceChangeRequest? Purchase = null,
    int PageIndex = 1,
    int PageSize = 50);

/// <summary>R12: the old or new prices of one version in a preview row.</summary>
public sealed record PriceTriple(decimal MrpAmount, decimal SaleAmount, decimal PurchaseAmount);

/// <summary>R12: why an item is not changed: it has no such version, or a result would go below zero or overflow.</summary>
public enum BulkSkipReason
{
    NoVersion,

    BelowZero,

    Overflow
}

/// <summary>R12: one row of the preview: the item, its old prices and the new ones, or why it is skipped.</summary>
public sealed record BulkPreviewRow(
    Guid ItemId, string ItemCode, string ItemName, Guid? VersionId, PriceTriple? Old, PriceTriple? New, BulkSkipReason? Skipped);

/// <summary>R12, R13: the totals of a preview or an apply.</summary>
public sealed record BulkTotals(int ItemsSelected, int Changed, int SkippedNoVersion, int SkippedBelowZero, int SkippedOverflow);

/// <summary>R12: one page of the preview and the totals; nothing was saved.</summary>
public sealed record BulkPreviewResult(IReadOnlyList<BulkPreviewRow> Rows, int RowCount, int PageIndex, int PageSize, BulkTotals Totals);
