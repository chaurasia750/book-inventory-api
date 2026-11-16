using BookInventory.Domain.Core;

namespace BookInventory.Domain.Purchase;

/// <summary>Spec purchase R4, R10: one line of a <see cref="VendorReturn"/>: the copied item, where it left from, the rate, the quantity and the amount.</summary>
public class VendorReturnLine : CompanyOwnedChild
{
    private VendorReturnLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
    }

    public Guid VendorReturnId { get; private set; }

    public int SortOrder { get; private set; }

    public Guid ItemId { get; private set; }

    public string ItemCode { get; private set; }

    public string ItemName { get; private set; }

    public ReturnStockSource StockSource { get; private set; }

    public decimal RateAmount { get; private set; }

    public int Quantity { get; private set; }

    public decimal LineAmount { get; private set; }

    internal static VendorReturnLine Of(Guid vendorReturnId, int sortOrder, VendorReturnLineInput input, decimal lineAmount) =>
        new()
        {
            VendorReturnId = vendorReturnId,
            SortOrder = sortOrder,
            ItemId = input.ItemId,
            ItemCode = input.ItemCode,
            ItemName = input.ItemName,
            StockSource = input.Source,
            RateAmount = input.Rate,
            Quantity = input.Quantity,
            LineAmount = lineAmount
        };
}
