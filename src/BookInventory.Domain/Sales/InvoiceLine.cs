using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R8, R13: one line of an <see cref="Invoice"/>: the copied snapshot, the delivered quantity, the applied discount and its amounts.</summary>
public class InvoiceLine : CompanyOwnedChild
{
    private InvoiceLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
        PriceVersionName = string.Empty;
    }

    public Guid InvoiceId { get; private set; }

    public Guid OrderLineId { get; private set; }

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

    public Discount ItemDiscount => new(ItemDiscountKind, ItemDiscountValue);

    public DeliveryLine Delivery => new(OrderLineId, new SaleItem(ItemId, ItemCode, ItemName, HsnCode, GstRatePercent), new SalePrice(PriceVersionId, PriceVersionName, MrpAmount, RateAmount), Quantity, ItemDiscount);

    internal static InvoiceLine Of(Guid invoiceId, int sortOrder, DeliveryLine d, SaleLineAmounts a) =>
        new()
        {
            InvoiceId = invoiceId,
            OrderLineId = d.OrderLineId,
            SortOrder = sortOrder,
            ItemId = d.Item.Id,
            ItemCode = d.Item.Code,
            ItemName = d.Item.Name,
            HsnCode = d.Item.HsnCode,
            GstRatePercent = d.Item.GstRatePercent,
            PriceVersionId = d.Price.VersionId,
            PriceVersionName = d.Price.VersionName,
            MrpAmount = d.Price.Mrp,
            RateAmount = d.Price.Rate,
            Quantity = d.Quantity,
            ItemDiscountKind = d.ItemDiscount.Kind,
            ItemDiscountValue = d.ItemDiscount.Value,
            GrossAmount = a.Gross,
            ItemDiscountAmount = a.ItemDiscount,
            LineAmount = a.LineAmount,
            TotalDiscountShareAmount = a.TotalDiscountShare,
            TaxableAmount = a.Taxable,
            CgstAmount = a.Cgst,
            SgstAmount = a.Sgst,
            IgstAmount = a.Igst
        };
}
