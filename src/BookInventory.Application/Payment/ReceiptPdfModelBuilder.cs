using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Application.Sales;
using BookInventory.Application.School;
using BookInventory.Domain.Payment;
using BookInventory.Domain.Sales;
using BookInventory.BuildingBlocks.Domain.Documents;

namespace BookInventory.Application.Payment;

/// <summary>
/// Spec payment R17: turns a receipt into the model the receipt template fills: every value is already formatted (Indian grouping,
/// dates) and HTML-encoded, so the template only places text. The company's name, contact, logo, signature and its one bank account
/// are the live company identity (company R17-R20); the amount in words is the same helper as the invoice's (sales R32).
/// </summary>
public static class ReceiptPdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(ReceiptDetail receipt, SchoolReference? school, CompanyPrintedIdentity? company)
    {
        var s = receipt.Summary;
        var cancelled = s.Status == ReceiptStatus.Cancelled;
        return new Dictionary<string, object?>
        {
            ["cancelled"] = cancelled,
            ["cancel_note"] = cancelled ? E(receipt.CancelReason) : null,
            ["company"] = Company(company),
            ["receipt"] = new Dictionary<string, object?>
            {
                ["number"] = E(s.Number), ["date"] = InvoiceFormat.Date(s.ReceiptDate), ["total"] = InvoiceFormat.Amount(s.TotalAmount), ["remark"] = E(receipt.Remark)
            },
            ["school"] = new Dictionary<string, object?> { ["name"] = E(s.SchoolName ?? school?.Name), ["address"] = E(school?.Address), ["gstin"] = E(school?.Gstin) },
            ["modes"] = receipt.Modes.Select(Mode).ToList(),
            ["words"] = E(AmountInWords.Of(s.TotalAmount)),
            ["has_allocations"] = receipt.Allocations.Count > 0,
            ["allocations"] = receipt.Allocations.Select(a => new Dictionary<string, object?> { ["invoice"] = E(a.InvoiceNumber ?? a.InvoiceId.ToString()), ["amount"] = InvoiceFormat.Money(a.Amount) }).ToList(),
            ["advance"] = receipt.AdvanceAmount > 0m ? InvoiceFormat.Money(receipt.AdvanceAmount) : null,
            ["has_pending_cheque"] = receipt.Modes.Any(m => m.ChequeStatus == ChequeStatus.Pending),
            ["bank"] = Bank(company),
            ["footer_line"] = E(company?.Invoice.Footer),
            ["signature"] = company?.Signature is { Length: > 0 } signature ? DataUri(company.SignatureContentType, signature) : null
        };
    }

    private static Dictionary<string, object?> Mode(ReceiptModeDto m) =>
        new() { ["mode"] = Label(m.Type), ["reference"] = Reference(m), ["amount"] = InvoiceFormat.Money(m.Amount) };

    private static string? Reference(ReceiptModeDto m) =>
        m.Type == ModeType.Cheque
            ? E($"Cheque {m.ChequeNumber} · {(m.ChequeDate is { } d ? InvoiceFormat.Date(d) : "-")} · {m.BankName}")
            : E(m.Reference);

    private static string Label(ModeType type) => type switch
    {
        ModeType.Cash => "Cash",
        ModeType.Upi => "UPI",
        ModeType.BankTransfer => "Bank transfer",
        _ => "Cheque"
    };

    private static Dictionary<string, object?> Company(CompanyPrintedIdentity? c) =>
        new()
        {
            ["name"] = E(c?.Name), ["tagline"] = E(c?.Tagline), ["address"] = E(c?.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", c?.Mobile), c?.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = c is { Invoice.PrintGstin: true } ? E(c.Gstin) : null,
            ["logo"] = c?.Logo is { Length: > 0 } logo ? DataUri(c.LogoContentType, logo) : null
        };

    /// <summary>Only the filled fields of the company's one bank account (company R19, payment R17).</summary>
    private static List<Dictionary<string, object?>> Bank(CompanyPrintedIdentity? c) =>
        c is null
            ? []
            : new[]
                {
                    ("Account name", c.Bank.AccountHolder), ("Bank", Join(c.Bank.BankName, c.Bank.Branch)), ("Account no.", c.Bank.AccountNumber), ("IFSC", c.Bank.Ifsc), ("UPI", c.Bank.UpiId)
                }
                .Where(f => !string.IsNullOrWhiteSpace(f.Item2))
                .Select(f => new Dictionary<string, object?> { ["label"] = f.Item1, ["value"] = E(f.Item2) })
                .ToList();

    private static string? Join(string? a, string? b) => string.Join(", ", new[] { a, b }.Where(v => !string.IsNullOrWhiteSpace(v)));

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());

    private static string DataUri(string? contentType, byte[] bytes) => string.Create(CultureInfo.InvariantCulture, $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}");
}
