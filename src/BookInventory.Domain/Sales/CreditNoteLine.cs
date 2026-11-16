using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R54, R55, R59: one line of a <see cref="CreditNote"/>: the copied snapshot, the returned quantity, its condition and its amounts.</summary>
public class CreditNoteLine : CompanyOwnedChild
{
    private CreditNoteLine()
    {
        ItemCode = string.Empty;
        ItemName = string.Empty;
        PriceVersionName = string.Empty;
    }

    public Guid CreditNoteId { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>R54: the invoice line this one returns (none without an invoice).</summary>
    public Guid? InvoiceLineId { get; private set; }

    /// <summary>R62: the order line the returned quantity goes back to (none without an invoice).</summary>
    public Guid? OrderLineId { get; private set; }

    public string? ClassName { get; private set; }

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

    public ReturnCondition Condition { get; private set; }

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

    internal static CreditNoteLine Of(Guid creditNoteId, int sortOrder, CreditNoteLineInput input, SaleLineAmounts a) =>
        new()
        {
            CreditNoteId = creditNoteId,
            SortOrder = sortOrder,
            InvoiceLineId = input.From?.InvoiceLineId,
            OrderLineId = input.From?.OrderLineId,
            ClassName = input.From?.ClassName,
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
            Condition = input.Condition,
            ItemDiscountKind = input.ItemDiscount.Kind,
            ItemDiscountValue = input.ItemDiscount.Value,
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
