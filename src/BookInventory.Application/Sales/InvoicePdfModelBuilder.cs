using System.Net;
using BookInventory.Application.Company;
using BookInventory.Domain.Core;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R30-R33: turns a printable invoice into the model the invoice templates fill: every value is already formatted
/// (Indian grouping, dates) and HTML-encoded, so the templates only place text and never format or escape. The bank account, terms
/// and header texts come from the company snapshot kept on the invoice (R13, D7); the logo and signature are the company's live
/// images.
/// </summary>
public static class InvoicePdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(PrintableInvoice printable, CompanyPrintedIdentity? live)
    {
        var d = printable.Detail;
        var c = printable.Company;
        var number = 0;
        return new Dictionary<string, object?>
        {
            ["title"] = Title(d),
            ["gst_exempt"] = !d.IsGstApplied,
            ["cancelled"] = d.Summary.Status == InvoiceStatus.Cancelled,
            ["company"] = Company(c, live),
            ["invoice"] = new Dictionary<string, object?>
            {
                ["number"] = E(d.Summary.Number), ["date"] = InvoiceFormat.Date(d.Summary.InvoiceDate),
                ["order"] = E(d.Summary.OrderNumber), ["session"] = E(d.Session), ["year"] = E(d.FinancialYear)
            },
            ["school"] = School(d),
            ["payment"] = d.PaymentType == PaymentType.Cash ? "Cash" : "Credit",
            ["show_bands"] = d.Layout == InvoiceLayout.ByClass,
            ["groups"] = d.Groups.Select(g => Group(g, ref number)).ToList(),
            ["line_count"] = d.Groups.Sum(g => g.Lines.Count),
            ["total_quantity"] = d.Lines.Sum(l => l.Quantity),
            ["has_summary"] = d.GstSummary.Count > 0,
            ["summary"] = d.GstSummary.Select(r => Summary(r, d.GstMode)).ToList(),
            ["same_state"] = d.GstMode == GstMode.SameState,
            ["totals"] = Totals(d),
            ["words"] = E(d.AmountInWords),
            ["bank"] = Bank(c),
            ["terms"] = Lines(c.Terms),
            ["footer_line"] = E(c.Footer),
            ["signature"] = live?.Signature is { Length: > 0 } sig ? DataUri(live.SignatureContentType, sig) : null
        };
    }

    /// <summary>R31: TAX INVOICE with GST; without it CASH MEMO or CREDIT MEMO by the payment type (R34).</summary>
    private static string Title(InvoiceDetail d) => d.IsGstApplied ? "TAX INVOICE" : d.PaymentType == PaymentType.Cash ? "CASH MEMO" : "CREDIT MEMO";

    private static Dictionary<string, object?> Company(CompanyInvoiceSnapshot c, CompanyPrintedIdentity? live) =>
        new()
        {
            ["name"] = E(c.Name), ["tagline"] = E(c.Tagline), ["address"] = E(c.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", c.Mobile), c.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = c.PrintGstin ? E(c.Gstin) : null, ["pan"] = E(c.Pan),
            ["logo"] = live?.Logo is { Length: > 0 } logo ? DataUri(live.LogoContentType, logo) : null
        };

    private static Dictionary<string, object?> School(InvoiceDetail d) =>
        new()
        {
            ["name"] = E(d.School.Name), ["code"] = E(d.School.Code), ["address"] = E(d.School.Address), ["state"] = E(d.School.State),
            ["gstin"] = E(d.SchoolGstin),
            ["tax_type"] = d.GstMode switch { GstMode.SameState => "CGST + SGST (same state)", GstMode.OtherState => "IGST (other state)", _ => "No GST" }
        };

    private static Dictionary<string, object?> Group(InvoiceGroupView g, ref int number)
    {
        var rows = new List<Dictionary<string, object?>>();
        foreach (var l in g.Lines)
        {
            number++;
            rows.Add(new Dictionary<string, object?>
            {
                ["no"] = number, ["name"] = E(l.Item.Name), ["code"] = E(l.Item.Code), ["hsn"] = E(l.Item.HsnCode),
                ["mrp"] = InvoiceFormat.Money(l.Price.Mrp), ["rate"] = InvoiceFormat.Money(l.Price.Rate), ["qty"] = l.Quantity,
                ["discount"] = l.Amounts.ItemDiscount > 0m ? InvoiceFormat.Money(l.Amounts.ItemDiscount) : "-",
                ["amount"] = InvoiceFormat.Money(l.Amounts.LineAmount)
            });
        }

        return new Dictionary<string, object?>
        {
            ["class_name"] = E(g.ClassName ?? "General"), ["lines"] = rows, ["subtotal"] = InvoiceFormat.Money(g.Lines.Sum(l => l.Amounts.LineAmount)),
            ["count"] = g.Lines.Count
        };
    }

    private static Dictionary<string, object?> Summary(GstRateSummaryRow r, GstMode mode) =>
        new()
        {
            ["hsn"] = E(r.HsnCode) ?? "-", ["rate"] = r.RatePercent.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            ["taxable"] = InvoiceFormat.Money(r.Taxable),
            ["cgst"] = InvoiceFormat.Money(r.Cgst), ["sgst"] = InvoiceFormat.Money(r.Sgst), ["igst"] = InvoiceFormat.Money(r.Igst),
            ["tax"] = InvoiceFormat.Money(r.Tax)
        };

    /// <summary>R32: with GST on: subtotal, total discount, taxable value, CGST + SGST or IGST, round-off, grand total; with GST off no tax row.</summary>
    private static List<Dictionary<string, object?>> Totals(InvoiceDetail d)
    {
        var t = d.Totals;
        var rows = new List<Dictionary<string, object?>> { Row("Subtotal", InvoiceFormat.Money(t.Subtotal)) };
        if (t.TotalDiscount > 0m)
            rows.Add(Row("Total discount" + (d.TotalDiscount.Kind == DiscountKind.Percent ? $" ({d.TotalDiscount.Value:0.##} %)" : string.Empty), "− " + InvoiceFormat.Money(t.TotalDiscount)));
        if (d.IsGstApplied)
        {
            rows.Add(Row("Taxable value", InvoiceFormat.Money(t.Taxable)));
            if (t.Cgst > 0m || d.GstMode == GstMode.SameState)
            {
                rows.Add(Row("CGST", InvoiceFormat.Money(t.Cgst)));
                rows.Add(Row("SGST", InvoiceFormat.Money(t.Sgst)));
            }

            if (t.Igst > 0m || d.GstMode == GstMode.OtherState)
                rows.Add(Row("IGST", InvoiceFormat.Money(t.Igst)));
        }

        if (t.RoundOff != 0m)
            rows.Add(Row("Round-off", (t.RoundOff < 0m ? "− " : "+ ") + InvoiceFormat.Money(Math.Abs(t.RoundOff))));
        rows.Add(Row("Grand total", "₹ " + InvoiceFormat.Money(t.GrandTotal), strong: true));
        return rows;
    }

    private static Dictionary<string, object?> Row(string label, string value, bool strong = false) =>
        new() { ["label"] = label, ["value"] = value, ["strong"] = strong };

    /// <summary>R33: only the filled fields, in the order a bank slip reads.</summary>
    private static List<Dictionary<string, object?>> Bank(CompanyInvoiceSnapshot c) =>
        new[]
        {
            ("Account name", c.BankAccountHolder), ("Bank", Join(c.BankName, c.BankBranch)), ("Account no.", c.BankAccountNumber), ("IFSC", c.BankIfsc), ("UPI", c.UpiId)
        }
        .Where(f => !string.IsNullOrWhiteSpace(f.Item2))
        .Select(f => new Dictionary<string, object?> { ["label"] = f.Item1, ["value"] = E(f.Item2) })
        .ToList();

    /// <summary>R33: one numbered item per non-empty line of the terms text; a number typed at the start of a line is dropped so the list does not number twice.</summary>
    private static List<string> Lines(string? text) =>
        (text ?? string.Empty).Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line => System.Text.RegularExpressions.Regex.Replace(line, @"^\d+[.)]\s*", string.Empty))
            .Select(E).OfType<string>().ToList();

    private static string? Join(string? a, string? b) => string.Join(", ", new[] { a, b }.Where(v => !string.IsNullOrWhiteSpace(v)));

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string DataUri(string? contentType, byte[] bytes) => $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}";

    /// <summary>HTML-encodes a value for the template; <c>null</c> and blanks stay <c>null</c> so the template can skip them.</summary>
    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());
}
