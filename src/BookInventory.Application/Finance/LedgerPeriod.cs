using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R27: the From and To dates printed on the ledger PDF, as text. A financial year is its April to March;
/// a date range is the dates asked, an open end being the first or last line of the statement; a session or no filter is
/// the first and the last line; a statement with no line reads a dash.
/// </summary>
public sealed record LedgerPeriod(string From, string To)
{
    private const string None = "-";

    public static LedgerPeriod Of(LedgerStatementQuery query, IReadOnlyList<LedgerStatementRow> rows)
    {
        if (FinancialYear.StartOf(query.FinancialYear?.Trim()) is { } start)
            return new LedgerPeriod(Text(start), Text(start.AddYears(1).AddDays(-1)));

        var from = query.From ?? (rows.Count > 0 ? rows[0].Date : (DateOnly?)null);
        var to = query.To ?? (rows.Count > 0 ? rows[^1].Date : (DateOnly?)null);
        return new LedgerPeriod(from is { } f ? Text(f) : None, to is { } t ? Text(t) : None);
    }

    private static string Text(DateOnly date) => Sales.InvoiceFormat.Date(date);
}
