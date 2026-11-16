using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R71: turns a credit note into the model the A4 credit note templates fill: every value is already formatted (Indian
/// grouping, dates) and HTML-encoded, so the templates only place text. The header text, GSTIN and footer are the credit note's own
/// snapshot (R59) so a reprint never changes; the logo and the signature are read live from the company (R17/R18). The GST summary
/// is the rate-wise one of R32.
/// </summary>
public static class CreditNotePdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(CreditNote note, CompanyPrintedIdentity? live)
    {
        var t = note.Totals;
        return new Dictionary<string, object?>
        {
            ["cancelled"] = note.Status == CreditNoteStatus.Cancelled,
            ["gst_exempt"] = !note.IsGstApplied,
            ["company"] = Company(note.CompanySnapshot, live),
            ["note"] = new Dictionary<string, object?>
            {
                ["number"] = E(note.Number), ["date"] = InvoiceFormat.Date(note.NoteDate), ["invoice"] = E(note.InvoiceNumber),
                ["session"] = E(note.Session), ["year"] = E(note.FinancialYear), ["reason"] = ReasonLabel(note.Reason), ["tax_type"] = TaxType(note.GstMode)
            },
            ["school"] = new Dictionary<string, object?> { ["name"] = E(note.SchoolName), ["code"] = E(note.SchoolCode), ["state"] = E(note.SchoolState), ["gstin"] = E(note.SchoolGstin) },
            ["lines"] = note.Lines.Select((l, i) => Line(l, i + 1)).ToList(),
            ["line_count"] = note.Lines.Count,
            ["total_quantity"] = note.Lines.Sum(l => l.Quantity),
            ["has_summary"] = note.IsGstApplied,
            ["same_state"] = note.GstMode == GstMode.SameState,
            ["summary"] = GstRateSummary.Of(note.Lines).Select(Summary).ToList(),
            ["totals"] = Totals(note),
            ["words"] = E(AmountInWords.Of(t.GrandTotal)),
            ["remark"] = E(note.Remark),
            ["cancel_note"] = note.Status == CreditNoteStatus.Cancelled ? E(note.CancelReason) : null,
            ["footer_line"] = E(note.CompanySnapshot.Footer),
            ["signature"] = live?.Signature is { Length: > 0 } signature ? DataUri(live.SignatureContentType, signature) : null
        };
    }

    private static string ReasonLabel(CreditNoteReason reason) =>
        reason switch { CreditNoteReason.SessionEnd => "Session end", CreditNoteReason.Damaged => "Damaged", CreditNoteReason.WrongBook => "Wrong book", _ => "Other" };

    private static string TaxType(GstMode mode) =>
        mode switch { GstMode.SameState => "CGST + SGST (same state)", GstMode.OtherState => "IGST (other state)", _ => "No GST" };

    private static Dictionary<string, object?> Line(CreditNoteLine l, int number) =>
        new()
        {
            ["no"] = number, ["name"] = E(l.ItemName), ["code"] = E(l.ItemCode), ["hsn"] = E(l.HsnCode),
            ["condition"] = l.Condition == ReturnCondition.Good ? "Good" : "Damaged", ["rate"] = InvoiceFormat.Money(l.RateAmount), ["qty"] = l.Quantity,
            ["discount"] = l.ItemDiscountAmount > 0m ? InvoiceFormat.Money(l.ItemDiscountAmount) : null, ["amount"] = InvoiceFormat.Money(l.LineAmount)
        };

    /// <summary>R71: subtotal, the total discount, the GST when applied, the round-off and the grand total.</summary>
    private static List<Dictionary<string, object?>> Totals(CreditNote note)
    {
        var t = note.Totals;
        var rows = new List<Dictionary<string, object?>> { Row("Subtotal", InvoiceFormat.Money(t.Subtotal)) };
        if (t.TotalDiscount > 0m)
            rows.Add(Row("Total discount" + (note.TotalDiscountKind == DiscountKind.Percent ? $" ({note.TotalDiscountValue:0.##} %)" : string.Empty), "- " + InvoiceFormat.Money(t.TotalDiscount)));
        if (note.IsGstApplied)
        {
            rows.Add(Row("Taxable value", InvoiceFormat.Money(t.Taxable)));
            rows.AddRange(note.GstMode == GstMode.SameState
                ? [Row("CGST", InvoiceFormat.Money(t.Cgst)), Row("SGST", InvoiceFormat.Money(t.Sgst))]
                : [Row("IGST", InvoiceFormat.Money(t.Igst))]);
        }

        if (t.RoundOff != 0m)
            rows.Add(Row("Round-off", (t.RoundOff < 0m ? "- " : "+ ") + InvoiceFormat.Money(Math.Abs(t.RoundOff))));
        rows.Add(Row("Grand total", "₹ " + InvoiceFormat.Money(t.GrandTotal), strong: true));
        return rows;
    }

    private static Dictionary<string, object?> Row(string label, string value, bool strong = false) => new() { ["label"] = label, ["value"] = value, ["strong"] = strong };

    private static Dictionary<string, object?> Summary(GstRateSummaryRow r) =>
        new()
        {
            ["hsn"] = E(r.HsnCode) ?? "-", ["rate"] = r.RatePercent.ToString("0.##", CultureInfo.InvariantCulture), ["taxable"] = InvoiceFormat.Money(r.Taxable),
            ["cgst"] = InvoiceFormat.Money(r.Cgst), ["sgst"] = InvoiceFormat.Money(r.Sgst), ["igst"] = InvoiceFormat.Money(r.Igst), ["tax"] = InvoiceFormat.Money(r.Tax)
        };

    private static Dictionary<string, object?> Company(CompanyInvoiceSnapshot s, CompanyPrintedIdentity? live) =>
        new()
        {
            ["name"] = E(s.Name), ["tagline"] = E(s.Tagline), ["address"] = E(s.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", s.Mobile), s.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = s.PrintGstin ? E(s.Gstin) : null, ["pan"] = E(s.Pan),
            ["logo"] = live?.Logo is { Length: > 0 } logo ? DataUri(live.LogoContentType, logo) : null
        };

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());

    private static string DataUri(string? contentType, byte[] bytes) =>
        string.Create(CultureInfo.InvariantCulture, $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}");
}
