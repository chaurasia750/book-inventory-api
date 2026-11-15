using BookInventory.Application.Sales;

namespace BookInventory.Application.Finance;

/// <summary>Spec finance R18: the statement's filter said in words on the PDF (<c>All entries</c>, <c>Financial year 2026-27</c>, ...), written once.</summary>
public static class LedgerFilterWords
{
    public static string Of(LedgerStatementQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.FinancialYear))
            return $"Financial year {query.FinancialYear.Trim()}";
        if (!string.IsNullOrWhiteSpace(query.Session))
            return $"Session {query.Session.Trim()}";
        return Range(query.From, query.To);
    }

    private static string Range(DateOnly? from, DateOnly? to) => (from, to) switch
    {
        (null, null) => "All entries",
        ({ } f, null) => $"From {InvoiceFormat.Date(f)}",
        (null, { } t) => $"Up to {InvoiceFormat.Date(t)}",
        ({ } f, { } t) => $"{InvoiceFormat.Date(f)} to {InvoiceFormat.Date(t)}"
    };
}
