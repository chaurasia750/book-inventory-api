using System.Globalization;
using System.Net;
using BookInventory.Application.Company;
using BookInventory.Application.Sales;
using BookInventory.Domain.Finance;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R18: turns a statement into the model the ledger template fills. Every value is already formatted (Indian
/// grouping, dates) and HTML-encoded, so the template only places text. The rows, the opening row and the totals are the
/// statement's own numbers, never recomputed here, so the PDF and the screen cannot disagree. The company is the live identity
/// (company R17-R20).
/// </summary>
public static class LedgerPdfModelBuilder
{
    public static IReadOnlyDictionary<string, object?> Build(LedgerStatement statement, LedgerPdfParty party, LedgerPdfHeading heading, CompanyPrintedIdentity? company) =>
        new Dictionary<string, object?>
        {
            ["company"] = Company(company),
            ["party"] = Party(party, statement),
            ["filter"] = E(heading.Filter),
            ["period_from"] = E(heading.Period.From),
            ["period_to"] = E(heading.Period.To),
            ["made_at"] = E(heading.MadeAt),
            ["opening"] = Balance(statement.OpeningBalance, statement.PartyType),
            ["rows"] = statement.Rows.Select(r => Row(r, statement.PartyType)).ToList(),
            ["total_debit"] = InvoiceFormat.Money(statement.TotalDebit),
            ["total_credit"] = InvoiceFormat.Money(statement.TotalCredit),
            ["closing"] = Balance(statement.ClosingBalance, statement.PartyType),
            ["summary_opening"] = StandingBalance(statement.OpeningBalance, statement.PartyType),
            ["summary_debit"] = InvoiceFormat.Amount(statement.TotalDebit),
            ["summary_credit"] = InvoiceFormat.Amount(statement.TotalCredit),
            ["summary_closing"] = StandingBalance(statement.ClosingBalance, statement.PartyType),
            ["words"] = E(AmountInWords.Of(Math.Abs(statement.ClosingBalance))),
            ["bank"] = statement.PartyType == PartyType.Vendor ? [] : Bank(company),
            ["footer_line"] = E(company?.Invoice.Footer),
            ["signature"] = company?.Signature is { Length: > 0 } signature ? DataUri(company.SignatureContentType, signature) : null
        };

    private static Dictionary<string, object?> Row(LedgerStatementRow row, PartyType type) =>
        new()
        {
            ["date"] = InvoiceFormat.Date(row.Date),
            ["document"] = E(row.DocumentNumber),
            ["detail"] = E(row.Narration),
            ["debit"] = row.DebitAmount > 0m ? InvoiceFormat.Money(row.DebitAmount) : null,
            ["credit"] = row.CreditAmount > 0m ? InvoiceFormat.Money(row.CreditAmount) : null,
            ["balance"] = Balance(row.Balance, type)
        };

    /// <summary>R21: the amount with Dr (the party owes the company) or Cr; a school owes when its balance is positive, a vendor when it is negative (R7); zero has no letter.</summary>
    private static string Balance(decimal balance, PartyType type) =>
        InvoiceFormat.Money(Math.Abs(balance)) + Letter(balance, type);

    /// <summary>Platform P26 with R21: a balance standing on its own, <c>₹ 1,200.00 Dr</c>.</summary>
    private static string StandingBalance(decimal balance, PartyType type) =>
        InvoiceFormat.Amount(Math.Abs(balance)) + Letter(balance, type);

    private static string Letter(decimal balance, PartyType type)
    {
        if (balance == 0m)
            return string.Empty;

        var partyOwes = type == PartyType.Vendor ? balance < 0m : balance > 0m;
        return partyOwes ? " Dr" : " Cr";
    }

    /// <summary>R23: only the filled fields of the company's one bank account, in the order a bank slip reads; none when the company has no account number.</summary>
    private static List<Dictionary<string, object?>> Bank(CompanyPrintedIdentity? c)
    {
        if (c is null || string.IsNullOrWhiteSpace(c.Bank.AccountNumber))
            return [];

        return new[]
            {
                ("Account name", c.Bank.AccountHolder), ("Bank", string.Join(", ", new[] { c.Bank.BankName, c.Bank.Branch }.Where(v => !string.IsNullOrWhiteSpace(v)))),
                ("Account no.", c.Bank.AccountNumber), ("IFSC", c.Bank.Ifsc), ("UPI", c.Bank.UpiId)
            }
            .Where(f => !string.IsNullOrWhiteSpace(f.Item2))
            .Select(f => new Dictionary<string, object?> { ["label"] = f.Item1, ["value"] = E(f.Item2) })
            .ToList();
    }

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
            ["pan"] = E(c?.Pan),
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
