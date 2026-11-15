namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R7: the document totals: subtotal, total discount, taxable value, GST parts, round-off and grand total.</summary>
public sealed record SaleTotals(
    decimal Subtotal,
    decimal TotalDiscount,
    decimal Taxable,
    decimal Cgst,
    decimal Sgst,
    decimal Igst,
    decimal RoundOff,
    decimal GrandTotal)
{
    public static readonly SaleTotals Zero = new(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);

    public decimal Gst => Cgst + Sgst + Igst;

    public decimal BeforeRounding => Taxable + Gst;
}
