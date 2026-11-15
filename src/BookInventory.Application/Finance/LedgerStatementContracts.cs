using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>R9: the statement filter: a financial year (<c>2026-27</c>), a session (schools only), or a date range; empty means all.</summary>
public sealed record LedgerStatementQuery
{
    public string? FinancialYear { get; init; }

    public string? Session { get; init; }

    public DateOnly? From { get; init; }

    public DateOnly? To { get; init; }
}

/// <summary>R9: one line of a statement, with the party's balance after it.</summary>
public sealed record LedgerStatementRow(
    DateOnly Date,
    string DocumentType,
    Guid DocumentId,
    string DocumentNumber,
    PostingKind PostingKind,
    string? Narration,
    string? Session,
    string FinancialYear,
    decimal DebitAmount,
    decimal CreditAmount,
    decimal Balance);

/// <summary>R9, R16: a party's statement: the party, the opening row, the lines, the totals and the closing balance.</summary>
public sealed record LedgerStatement(
    PartyType PartyType,
    Guid PartyId,
    string PartyCode,
    string PartyName,
    bool IsActive,
    decimal OpeningBalance,
    IReadOnlyList<LedgerStatementRow> Rows,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal ClosingBalance);

/// <summary>R11: the outstanding list search and paging.</summary>
public sealed record OutstandingQuery
{
    /// <summary>Free text over the party's name and code.</summary>
    public string? Search { get; init; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R11, R16: one party with a non-zero balance.</summary>
public sealed record OutstandingRow(Guid PartyId, string Code, string Name, bool IsActive, decimal Balance);

/// <summary>R11: one page of the outstanding list and the total of the whole list.</summary>
public sealed record OutstandingPage(
    IReadOnlyList<OutstandingRow> Items,
    int PageIndex,
    int PageSize,
    int TotalCount,
    int TotalPages,
    decimal TotalBalance);
