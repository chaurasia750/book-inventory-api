using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Application.Sales;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R18: turns a statement into the model the ledger template fills. Every value is already formatted (Indian
/// grouping, dates) and HTML-encoded, so the template only places text. The rows, the opening row and the totals are the
/// statement's own numbers, never recomputed here, so the PDF and the screen cannot disagree. The company is the live identity
/// (company R17-R20).
/// </summary>
public static class LedgerPdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(LedgerStatement statement, LedgerPdfParty party, string filter, CompanyPrintedIdentity? company, string madeAt) =>
        new Dictionary<string, object?>
        {
            ["company"] = Company(company),
            ["party"] = Party(party, statement),
            ["filter"] = E(filter),
            ["made_at"] = E(madeAt),
            ["opening"] = InvoiceFormat.Money(statement.OpeningBalance),
            ["rows"] = statement.Rows.Select(Row).ToList(),
            ["total_debit"] = InvoiceFormat.Money(statement.TotalDebit),
            ["total_credit"] = InvoiceFormat.Money(statement.TotalCredit),
            ["closing"] = InvoiceFormat.Money(statement.ClosingBalance)
        };

    private static Dictionary<string, object?> Row(LedgerStatementRow row) =>
        new()
        {
            ["date"] = InvoiceFormat.Date(row.Date),
            ["document"] = E(row.DocumentNumber),
            ["detail"] = E(row.Narration),
            ["debit"] = row.DebitAmount > 0m ? InvoiceFormat.Money(row.DebitAmount) : null,
            ["credit"] = row.CreditAmount > 0m ? InvoiceFormat.Money(row.CreditAmount) : null,
            ["balance"] = InvoiceFormat.Money(row.Balance)
        };

    private static Dictionary<string, object?> Party(LedgerPdfParty party, LedgerStatement statement) =>
        new()
        {
            ["name"] = E(party.Name),
            ["code"] = E(party.Code),
            ["address"] = E(party.Address),
            ["state"] = E(party.State),
            ["gstin"] = E(party.Gstin),
            ["kind"] = statement.PartyType.ToString().ToUpperInvariant()
        };

    private static Dictionary<string, object?> Company(CompanyPrintedIdentity? c) =>
        new()
        {
            ["name"] = E(c?.Name),
            ["address"] = E(c?.Address),
            ["contact"] = E(string.Join(" · ", new[] { Prefixed("Mobile", c?.Mobile), c?.Email }.Where(v => !string.IsNullOrWhiteSpace(v)))),
            ["gstin"] = c is { Invoice.PrintGstin: true } ? E(c.Gstin) : null,
            ["logo"] = c?.Logo is { Length: > 0 } logo ? DataUri(c.LogoContentType, logo) : null
        };

    private static string? Prefixed(string label, string? value) => string.IsNullOrWhiteSpace(value) ? null : $"{label} {value}";

    private static string? E(string? value) => string.IsNullOrWhiteSpace(value) ? null : WebUtility.HtmlEncode(value.Trim());

    private static string DataUri(string? contentType, byte[] bytes) =>
        string.Create(CultureInfo.InvariantCulture, $"data:{contentType ?? "image/png"};base64,{Convert.ToBase64String(bytes)}");
}
