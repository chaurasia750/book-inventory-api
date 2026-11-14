using BookInventory.Domain.Core;

namespace BookInventory.Domain.Item;

/// <summary>
/// R6: one barcode of one item. A company-owned child (platform P18) of <see cref="Item"/>: it is added and removed only through
/// the root, which keeps the barcodes of an item distinct. A removed barcode is soft-deleted so it can be given to another item.
/// </summary>
public class ItemBarcode : CompanyOwnedChild
{
    public const int MaxLength = 30;

    private ItemBarcode()
    {
        Barcode = string.Empty;
    }

    public Guid ItemId { get; private set; }

    /// <summary>The barcode in upper case.</summary>
    public string Barcode { get; private set; }

    internal static ItemBarcode Of(Guid itemId, string barcode) => new() { ItemId = itemId, Barcode = barcode };
}
