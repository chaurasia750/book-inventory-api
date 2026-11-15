using BookInventory.Domain.Core;
using BookInventory.Domain.Item;

namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R43, R44: one line of a <see cref="CounterBill"/>: the copied snapshot, the quantity, the discount and its amounts.</summary>
public class CounterBillLine : CompanyOwnedChild
{
    private CounterBillLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
        PriceVersionName = string.Empty;
    }

    public Guid CounterBillId { get; private set; }

    public int SortOrder { get; private set; }

    public Guid ItemId { get; private set; }

    public string ItemCode { get; private set; }

    public string ItemName { get; private set; }

    public string? HsnCode { get; private set; }

    public decimal GstRatePercent { get; private set; }

    /// <summary>R50: the item's category; the books count of the day summary adds the quantity of the <c>Book</c> lines.</summary>
    public ItemCategory Category { get; private set; }

    public Guid PriceVersionId { get; private set; }

    public string PriceVersionName { get; private set; }

    public decimal MrpAmount { get; private set; }

    public int Quantity { get; private set; }

    public DiscountKind ItemDiscountKind { get; private set; }

    public decimal ItemDiscountValue { get; private set; }

    public decimal GrossAmount { get; private set; }

    public decimal ItemDiscountAmount { get; private set; }

    public decimal LineAmount { get; private set; }

    public decimal TotalDiscountShareAmount { get; private set; }

    public decimal TaxableAmount { get; private set; }

    public decimal CgstAmount { get; private set; }

    public decimal SgstAmount { get; private set; }

    public decimal IgstAmount { get; private set; }

    internal static CounterBillLine Of(Guid billId, int sortOrder, CounterLineInput input, SaleLineAmounts amounts) =>
        new()
        {
            CounterBillId = billId,
            SortOrder = sortOrder,
            ItemId = input.Item.Id,
            ItemCode = input.Item.Code,
            ItemName = input.Item.Name,
            HsnCode = input.Item.HsnCode,
            GstRatePercent = input.Item.GstRatePercent,
            Category = input.Category,
            PriceVersionId = input.Price.VersionId,
            PriceVersionName = input.Price.VersionName,
            MrpAmount = input.Price.Mrp,
            Quantity = input.Quantity,
            ItemDiscountKind = input.ItemDiscount.Kind,
            ItemDiscountValue = input.ItemDiscount.Value,
            GrossAmount = amounts.Gross,
            ItemDiscountAmount = amounts.ItemDiscount,
            LineAmount = amounts.LineAmount,
            TotalDiscountShareAmount = amounts.TotalDiscountShare,
            TaxableAmount = amounts.Taxable,
            CgstAmount = amounts.Cgst,
            SgstAmount = amounts.Sgst,
            IgstAmount = amounts.Igst
        };
}
