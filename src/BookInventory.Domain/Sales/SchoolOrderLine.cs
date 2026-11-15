using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R3, R7, R8: one line of a <see cref="SchoolOrder"/>: the item and price version snapshot, the quantity, the item
/// discount, the amounts computed by <see cref="SaleCalculation"/> and how much of it the live invoices hold. A company-owned child
/// (platform P18), changed only by its order.
/// </summary>
public class SchoolOrderLine : CompanyOwnedChild
{
    private SchoolOrderLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
        PriceVersionName = string.Empty;
    }

    public Guid SchoolOrderId { get; private set; }

    public int SortOrder { get; private set; }

    public Guid ItemId { get; private set; }

    public string ItemCode { get; private set; }

    public string ItemName { get; private set; }

    public string? HsnCode { get; private set; }

    public decimal GstRatePercent { get; private set; }

    public Guid PriceVersionId { get; private set; }

    public string PriceVersionName { get; private set; }

    public decimal MrpAmount { get; private set; }

    public decimal RateAmount { get; private set; }

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

    /// <summary>R8: the quantity the order's live invoices hold.</summary>
    public int InvoicedQuantity { get; private set; }

    /// <summary>R8: the part of a rupee item discount the live invoices already applied.</summary>
    public decimal ItemDiscountGiven { get; private set; }

    public int RemainingQuantity => Quantity - InvoicedQuantity;

    public Discount ItemDiscount => new(ItemDiscountKind, ItemDiscountValue);

    public SaleItem Item => new(ItemId, ItemCode, ItemName, HsnCode, GstRatePercent);

    public SalePrice Price => new(PriceVersionId, PriceVersionName, MrpAmount, RateAmount);

    public SaleLineInput CalculationInput => new(RateAmount, Quantity, ItemDiscount, GstRatePercent);

    internal static SchoolOrderLine Of(Guid orderId, int sortOrder, OrderLineInput input) =>
        new()
        {
            SchoolOrderId = orderId,
            SortOrder = sortOrder,
            ItemId = input.Item.Id,
            ItemCode = input.Item.Code,
            ItemName = input.Item.Name,
            HsnCode = input.Item.HsnCode,
            GstRatePercent = input.Item.GstRatePercent,
            PriceVersionId = input.Price.VersionId,
            PriceVersionName = input.Price.VersionName,
            MrpAmount = input.Price.Mrp,
            RateAmount = input.Price.Rate,
            Quantity = input.Quantity,
            ItemDiscountKind = input.ItemDiscount.Kind,
            ItemDiscountValue = input.ItemDiscount.Value
        };

    internal void SetAmounts(SaleLineAmounts amounts)
    {
        GrossAmount = amounts.Gross;
        ItemDiscountAmount = amounts.ItemDiscount;
        LineAmount = amounts.LineAmount;
        TotalDiscountShareAmount = amounts.TotalDiscountShare;
        TaxableAmount = amounts.Taxable;
        CgstAmount = amounts.Cgst;
        SgstAmount = amounts.Sgst;
        IgstAmount = amounts.Igst;
    }

    /// <summary>
    /// R8 (owner's answer Q10): the item discount of <paramref name="quantity"/> delivered now: a percentage as it is; a rupee amount
    /// shared by quantity, the delivery that empties the line taking what is left.
    /// </summary>
    internal Discount DiscountFor(int quantity)
    {
        if (ItemDiscountKind != DiscountKind.Amount)
            return ItemDiscount;

        var amount = quantity == RemainingQuantity ? ItemDiscountValue - ItemDiscountGiven : SaleMoney.Round2(ItemDiscountValue * quantity / Quantity);
        return new Discount(DiscountKind.Amount, amount);
    }

    /// <summary>R8, R12: an invoice takes (positive) or a cancel gives back (negative) a quantity and its rupee item discount.</summary>
    internal void ChangeInvoiced(int quantity, decimal itemDiscountAmount)
    {
        InvoicedQuantity = Math.Clamp(InvoicedQuantity + quantity, 0, Quantity);
        ItemDiscountGiven = Math.Max(0m, ItemDiscountGiven + itemDiscountAmount);
    }
}
