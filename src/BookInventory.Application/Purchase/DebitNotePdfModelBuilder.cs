using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Application.Sales;
using BookInventory.Domain.Purchase;
using BookInventory.Domain.Sales;
using BookInventory.BuildingBlocks.Domain.Documents;

namespace BookInventory.Application.Purchase;

/// <summary>
/// Spec purchase R11: turns a vendor return into the model the A4 Debit Note templates fill: every value is already formatted (Indian
/// grouping, dates) and HTML-encoded, so the templates only place text. The header text, GSTIN and footer are the vendor return's own
/// snapshot (R10) so a reprint never changes; the logo and the signature are read live from the company (R17/R18).
/// </summary>
public static class DebitNotePdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(VendorReturn vendorReturn, CompanyPrintedIdentity? live) =>
        new Dictionary<string, object?>
        {
            ["cancelled"] = vendorReturn.Status == VendorReturnStatus.Cancelled,
            ["company"] = Company(vendorReturn.CompanySnapshot, live),
            ["note"] = new Dictionary<string, object?> { ["number"] = E(vendorReturn.Number), ["date"] = InvoiceFormat.Date(vendorReturn.ReturnDate), ["year"] = E(vendorReturn.FinancialYear) },
            ["vendor"] = new Dictionary<string, object?> { ["name"] = E(vendorReturn.VendorName), ["code"] = E(vendorReturn.VendorCode) },
            ["lines"] = vendorReturn.Lines.Select((l, i) => Line(l, i + 1)).ToList(),
            ["line_count"] = vendorReturn.Lines.Count,
            ["total_quantity"] = vendorReturn.Lines.Sum(l => l.Quantity),
            ["totals"] = Totals(vendorReturn),
            ["words"] = E(AmountInWords.Of(vendorReturn.GrandTotalAmount)),
            ["remark"] = E(vendorReturn.Remark),
            ["cancel_note"] = vendorReturn.Status == VendorReturnStatus.Cancelled ? E(vendorReturn.CancelReason) : null,
            ["footer_line"] = E(vendorReturn.CompanySnapshot.Footer),
            ["signature"] = live?.Signature is { Length: > 0 } signature ? DataUri(live.SignatureContentType, signature) : null
        };

    private static Dictionary<string, object?> Line(VendorReturnLine l, int number) =>
        new()
        {
            ["no"] = number, ["name"] = E(l.ItemName), ["code"] = E(l.ItemCode), ["source"] = l.StockSource == ReturnStockSource.Main ? "Main" : "Damaged",
            ["rate"] = InvoiceFormat.Money(l.RateAmount), ["qty"] = l.Quantity, ["amount"] = InvoiceFormat.Money(l.LineAmount)
        };

    /// <summary>R11: the subtotal, the round-off when there is one and the total.</summary>
    private static List<Dictionary<string, object?>> Totals(VendorReturn r)
    {
        var rows = new List<Dictionary<string, object?>> { Row("Subtotal", InvoiceFormat.Amount(r.SubtotalAmount)) };
        if (r.RoundOffAmount != 0m)
            rows.Add(Row("Round-off", (r.RoundOffAmount < 0m ? "- " : "+ ") + InvoiceFormat.Amount(Math.Abs(r.RoundOffAmount))));
        rows.Add(Row("Total", InvoiceFormat.Amount(r.GrandTotalAmount), strong: true));
        return rows;
    }

    private static Dictionary<string, object?> Row(string label, string value, bool strong = false) => new() { ["label"] = label, ["value"] = value, ["strong"] = strong };

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
