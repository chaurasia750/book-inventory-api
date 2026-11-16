using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R51: turns a counter bill into the model the A5 receipt template fills: every value is already formatted (Indian
/// grouping, dates) and HTML-encoded, so the template only places text. The header text, GSTIN and footer are the bill's own snapshot
/// (R13) so a reprint never changes; the logo and the signature are read live from the company (R17/R18). The title follows R31: TAX
/// INVOICE with GST, CASH MEMO with a <c>GST Exempted</c> note without it. The GST summary is the rate-wise one of R32.
/// </summary>
public static class CounterBillPdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(CounterBill bill, string? counterName, CompanyPrintedIdentity? live)
    {
        var t = bill.Totals;
        return new Dictionary<string, object?>
        {
            ["cancelled"] = bill.Status == CounterBillStatus.Cancelled,
            ["title"] = bill.IsGstApplied ? "TAX INVOICE" : "CASH MEMO",
            ["gst_exempt"] = !bill.IsGstApplied,
            ["company"] = Company(bill.CompanySnapshot, live),
            ["bill"] = new Dictionary<string, object?>
            {
                ["number"] = E(bill.Number), ["date"] = InvoiceFormat.Date(bill.BillDate), ["counter"] = E(counterName), ["session"] = E(bill.Session)
            },
            ["school"] = new Dictionary<string, object?> { ["name"] = E(bill.SchoolName), ["code"] = E(bill.SchoolCode), ["gstin"] = E(bill.SchoolGstin) },
            ["student"] = new Dictionary<string, object?>
            {
                ["name"] = E(bill.StudentName), ["class"] = E(bill.StudentClass), ["section"] = E(bill.StudentSection), ["roll"] = E(bill.StudentRollNumber)
            },
            ["lines"] = bill.Lines.Select((l, i) => Line(l, i + 1)).ToList(),
            ["totals"] = Totals(bill),
            ["has_summary"] = bill.IsGstApplied,
            ["same_state"] = bill.GstMode == GstMode.SameState,
            ["summary"] = GstRateSummary.Of(bill.Lines).Select(Summary).ToList(),
            ["grand_total"] = InvoiceFormat.Money(t.GrandTotal),
            ["words"] = E(AmountInWords.Of(t.GrandTotal)),
            ["payments"] = bill.Payments.Select(Payment).ToList(),
            ["remark"] = E(bill.Remark),
            ["cancel_note"] = bill.Status == CounterBillStatus.Cancelled ? E(bill.CancelReason) : null,
            ["footer_line"] = E(bill.CompanySnapshot.Footer),
            ["signature"] = live?.Signature is { Length: > 0 } signature ? DataUri(live.SignatureContentType, signature) : null
        };
    }

    private static Dictionary<string, object?> Line(CounterBillLine l, int number) =>
        new()
        {
            ["n"] = number, ["name"] = E(l.ItemName), ["code"] = E(l.ItemCode), ["qty"] = l.Quantity, ["mrp"] = InvoiceFormat.Money(l.MrpAmount),
            ["discount"] = l.ItemDiscountAmount > 0m ? InvoiceFormat.Money(l.ItemDiscountAmount) : null, ["amount"] = InvoiceFormat.Money(l.LineAmount)
        };

    /// <summary>R51: subtotal, the total discount, the GST (taxable value and CGST + SGST or IGST) when GST is applied, and the round-off.</summary>
    private static List<Dictionary<string, object?>> Totals(CounterBill bill)
    {
        var t = bill.Totals;
        var rows = new List<Dictionary<string, object?>> { Row("Subtotal", InvoiceFormat.Money(t.Subtotal)) };
        if (t.TotalDiscount > 0m)
            rows.Add(Row("Total discount" + (bill.TotalDiscountKind == DiscountKind.Percent ? $" ({bill.TotalDiscountValue:0.##} %)" : string.Empty), "-" + InvoiceFormat.Money(t.TotalDiscount)));
        if (bill.IsGstApplied)
        {
            rows.Add(Row("Taxable value", InvoiceFormat.Money(t.Taxable)));
            if (bill.GstMode == GstMode.SameState)
            {
                rows.Add(Row("CGST", InvoiceFormat.Money(t.Cgst)));
                rows.Add(Row("SGST", InvoiceFormat.Money(t.Sgst)));
            }
            else
            {
                rows.Add(Row("IGST", InvoiceFormat.Money(t.Igst)));
            }
        }

        if (t.RoundOff != 0m)
            rows.Add(Row("Round off", InvoiceFormat.Money(t.RoundOff)));
        return rows;
    }

    private static Dictionary<string, object?> Row(string label, string value) => new() { ["label"] = label, ["value"] = value };

    private static Dictionary<string, object?> Summary(GstRateSummaryRow r) =>
        new()
        {
            ["hsn"] = E(r.HsnCode) ?? "-", ["rate"] = r.RatePercent.ToString("0.##", CultureInfo.InvariantCulture), ["taxable"] = InvoiceFormat.Money(r.Taxable),
            ["cgst"] = InvoiceFormat.Money(r.Cgst), ["sgst"] = InvoiceFormat.Money(r.Sgst), ["igst"] = InvoiceFormat.Money(r.Igst), ["tax"] = InvoiceFormat.Money(r.Tax)
        };

    private static Dictionary<string, object?> Payment(CounterBillPayment p) =>
        new() { ["mode"] = p.Mode == CounterPaymentMode.Cash ? "Cash" : "UPI", ["reference"] = E(p.Reference), ["amount"] = InvoiceFormat.Money(p.Amount) };

    private static Dictionary<string, object?> Company(CompanyInvoiceSnapshot s, CompanyPrintedIdentity? live) =>
        new()
        {
            ["name"] = E(s.Name), ["tagline"] = E(s.Tagline), ["address"] = E(s.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", s.Mobile), s.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = s.PrintGstin ? E(s.Gstin) : null,
            ["logo"] = live?.Logo is { Length: > 0 } logo ? DataUri(live.LogoContentType, logo) : null
        };

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());

    private static string DataUri(string? contentType, byte[] bytes) =>
        string.Create(CultureInfo.InvariantCulture, $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}");
}
