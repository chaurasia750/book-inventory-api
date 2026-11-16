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
        Rows(lines.Select(l => new TaxedLine(l.HsnCode, l.GstRatePercent, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount)));

    /// <summary>R51: the same rate-wise summary for the lines of a counter bill, by the same grouping.</summary>
    public static IReadOnlyList<GstRateSummaryRow> Of(IReadOnlyList<CounterBillLine> lines) =>
        Rows(lines.Select(l => new TaxedLine(l.HsnCode, l.GstRatePercent, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount)));

    /// <summary>R71: the same rate-wise summary for the lines of a credit note, by the same grouping.</summary>
    public static IReadOnlyList<GstRateSummaryRow> Of(IReadOnlyList<CreditNoteLine> lines) =>
        Rows(lines.Select(l => new TaxedLine(l.HsnCode, l.GstRatePercent, l.TaxableAmount, l.CgstAmount, l.SgstAmount, l.IgstAmount)));

    private static IReadOnlyList<GstRateSummaryRow> Rows(IEnumerable<TaxedLine> lines) =>
        [.. lines
            .GroupBy(l => (l.HsnCode, l.RatePercent))
            .OrderBy(g => g.Key.RatePercent).ThenBy(g => g.Key.HsnCode)
            .Select(g => new GstRateSummaryRow(g.Key.HsnCode, g.Key.RatePercent, g.Sum(l => l.Taxable), g.Sum(l => l.Cgst), g.Sum(l => l.Sgst), g.Sum(l => l.Igst)))];

    private sealed record TaxedLine(string? HsnCode, decimal RatePercent, decimal Taxable, decimal Cgst, decimal Sgst, decimal Igst);
}
