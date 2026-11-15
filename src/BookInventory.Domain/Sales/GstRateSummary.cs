namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R32: one rate-wise row of the invoice GST summary: the taxable value and the tax of one HSN/rate.</summary>
public sealed record GstRateSummaryRow(
    string? HsnCode,
    decimal RatePercent,
    decimal Taxable,
    decimal Cgst,
    decimal Sgst,
    decimal Igst)
{
    /// <summary>The tax of this rate: CGST + SGST for a same-state invoice, IGST for another state, zero when GST is off.</summary>
    public decimal Tax => Cgst + Sgst + Igst;
}

/// <summary>
/// Spec sales R32 (owner's answer Q15, D7): the GST of an invoice summed by HSN and rate, so the printed and on-screen
/// invoice show one summary row per rate and never a per-line GST column. The line computation of R7 is untouched: this
/// only adds up the per-line amounts it already produced.
/// </summary>
public static class GstRateSummary
{
    /// <summary>The rows, lowest rate first (then by HSN); the sum of <see cref="GstRateSummaryRow.Tax"/> equals the invoice's total tax.</summary>
    public static IReadOnlyList<GstRateSummaryRow> Of(IReadOnlyList<InvoiceLine> lines) =>
        [.. lines
            .GroupBy(l => (l.HsnCode, l.GstRatePercent))
            .OrderBy(g => g.Key.GstRatePercent).ThenBy(g => g.Key.HsnCode)
            .Select(g => new GstRateSummaryRow(
                g.Key.HsnCode,
                g.Key.GstRatePercent,
                g.Sum(l => l.TaxableAmount),
                g.Sum(l => l.CgstAmount),
                g.Sum(l => l.SgstAmount),
                g.Sum(l => l.IgstAmount)))];
}
