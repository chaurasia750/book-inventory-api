using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Finance;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Finance;

/// <summary>
/// The read side of spec finance R9 and R11 over the lines (reached through their transactions). For a session filter the opening
/// row is every line of an earlier session (the session text orders: <c>2026-27</c> before <c>2027-28</c>) and every line without
/// a session; for a date filter every line dated before it.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ILedgerStatementStore))]
public sealed class LedgerStatementStore(IUnitOfWork unitOfWork) : ILedgerStatementStore
{
    private readonly IRepository<LedgerTransaction> _transactions = unitOfWork.Repository<LedgerTransaction>();

    /// <summary>A line with its transaction; a member-init shape (not a positional record) so EF can filter and sort through it.</summary>
    private sealed class Row
    {
        public required LedgerTransaction Transaction { get; init; }

        public required LedgerLine Line { get; init; }
    }

    public async Task<IReadOnlyList<StatementLine>> LinesAsync(LedgerParty party, StatementFilter filter, int limit, CancellationToken cancellationToken = default)
    {
        var rows = OfParty(party);
        if (filter.Session is { } session)
            rows = rows.Where(r => r.Line.Session == session);
        if (filter.From is { } from)
            rows = rows.Where(r => r.Line.TransactionDate >= from);
        if (filter.Until is { } until)
            rows = rows.Where(r => r.Line.TransactionDate < until);

        return await rows
            .OrderBy(r => r.Line.TransactionDate).ThenBy(r => r.Transaction.CreatedAt).ThenBy(r => r.Line.Id)
            .Take(limit)
            .Select(r => new StatementLine(
                r.Line.TransactionDate, r.Transaction.DocumentType, r.Transaction.DocumentId, r.Transaction.DocumentNumber, r.Transaction.PostingKind,
                r.Transaction.Narration, r.Line.Session, r.Line.FinancialYear, r.Line.DebitAmount, r.Line.CreditAmount))
            .ToListAsync(cancellationToken);
    }

    public async Task<PartyTotals> TotalsBeforeAsync(LedgerParty party, StatementFilter filter, CancellationToken cancellationToken = default)
    {
        var rows = OfParty(party);
        if (filter.Session is { } session)
            rows = rows.Where(r => r.Line.Session == null || string.Compare(r.Line.Session, session) < 0);
        else if (filter.From is { } from)
            rows = rows.Where(r => r.Line.TransactionDate < from);
        else
            return new PartyTotals(party.Id, 0m, 0m);

        var debit = await rows.SumAsync(r => r.Line.DebitAmount, cancellationToken);
        var credit = await rows.SumAsync(r => r.Line.CreditAmount, cancellationToken);
        return new PartyTotals(party.Id, debit, credit);
    }

    public async Task<IReadOnlyList<PartyTotals>> AllTotalsAsync(PartyType partyType, CancellationToken cancellationToken = default) =>
        await _transactions.TableNoTracking
            .SelectMany(t => t.Lines)
            .Where(l => l.PartyType == partyType && l.PartyId != null)
            .GroupBy(l => l.PartyId!.Value)
            .Select(g => new PartyTotals(g.Key, g.Sum(l => l.DebitAmount), g.Sum(l => l.CreditAmount)))
            .ToListAsync(cancellationToken);

    private IQueryable<Row> OfParty(LedgerParty party) =>
        _transactions.TableNoTracking
            .SelectMany(t => t.Lines, (t, l) => new Row { Transaction = t, Line = l })
            .Where(r => r.Line.PartyType == party.Type && r.Line.PartyId == party.Id);
}
