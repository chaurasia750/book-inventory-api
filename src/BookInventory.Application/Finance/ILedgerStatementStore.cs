using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>R9: which lines of a party a statement reads: dated from <see cref="From"/> (inclusive) to <see cref="Until"/> (exclusive), of one session, or all.</summary>
public sealed record StatementFilter(DateOnly? From, DateOnly? Until, string? Session);

/// <summary>R9: one line of a party with its transaction's document.</summary>
public sealed record StatementLine(
    DateOnly Date,
    string DocumentType,
    Guid DocumentId,
    string DocumentNumber,
    PostingKind PostingKind,
    string? Narration,
    string? Session,
    string FinancialYear,
    decimal DebitAmount,
    decimal CreditAmount);

/// <summary>The read side of the statement and the outstanding lists (R9, R11). Company-filtered like every store.</summary>
public interface ILedgerStatementStore
{
    /// <summary>The party's lines inside the filter, oldest first (date, then posting time), at most <paramref name="limit"/>.</summary>
    Task<IReadOnlyList<StatementLine>> LinesAsync(LedgerParty party, StatementFilter filter, int limit, CancellationToken cancellationToken = default);

    /// <summary>R9: the debit and credit sums of the party's lines before the filter (the opening row).</summary>
    Task<PartyTotals> TotalsBeforeAsync(LedgerParty party, StatementFilter filter, CancellationToken cancellationToken = default);

    /// <summary>R11: the debit and credit sums of every party of one type that has lines.</summary>
    Task<IReadOnlyList<PartyTotals>> AllTotalsAsync(PartyType partyType, CancellationToken cancellationToken = default);
}
