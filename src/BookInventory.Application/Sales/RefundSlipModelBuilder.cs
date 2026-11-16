using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// Spec sales R68: turns a refund into the model the A5 refund slip template fills: every value is already formatted (Indian grouping,
/// dates) and HTML-encoded, so the template only places text. The company header, logo and signature are read live from the company
/// (R17/R18); the amount is also given in words.
/// </summary>
public static class RefundSlipModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(SchoolRefund refund, CompanyPrintedIdentity? company) =>
        new Dictionary<string, object?>
        {
            ["cancelled"] = refund.Status == RefundStatus.Cancelled,
            ["company"] = Company(company),
            ["refund"] = new Dictionary<string, object?>
            {
                ["number"] = E(refund.Number),
                ["date"] = InvoiceFormat.Date(refund.RefundDate),
                ["mode"] = ModeLabel(refund.Mode),
                ["reference"] = E(refund.Reference),
                ["credit_note"] = E(refund.CreditNoteNumber),
                ["amount"] = InvoiceFormat.Money(refund.Amount)
            },
            ["school"] = new Dictionary<string, object?> { ["name"] = E(refund.SchoolName), ["code"] = E(refund.SchoolCode) },
            ["words"] = E(AmountInWords.Of(refund.Amount)),
            ["remark"] = E(refund.Remark),
            ["cancel_note"] = refund.Status == RefundStatus.Cancelled ? E(refund.CancelReason) : null,
            ["signature"] = company?.Signature is { Length: > 0 } signature ? DataUri(company.SignatureContentType, signature) : null
        };

    private static string ModeLabel(RefundMode mode) => mode switch { RefundMode.Cash => "Cash", RefundMode.Upi => "UPI", _ => "Bank transfer" };

    private static Dictionary<string, object?> Company(CompanyPrintedIdentity? c) =>
        new()
        {
            ["name"] = E(c?.Name), ["tagline"] = E(c?.Tagline), ["address"] = E(c?.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", c?.Mobile), c?.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = c is { Invoice.PrintGstin: true } ? E(c.Gstin) : null,
            ["logo"] = c?.Logo is { Length: > 0 } logo ? DataUri(c.LogoContentType, logo) : null
        };

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());

    private static string DataUri(string? contentType, byte[] bytes) =>
        string.Create(CultureInfo.InvariantCulture, $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}");
}
