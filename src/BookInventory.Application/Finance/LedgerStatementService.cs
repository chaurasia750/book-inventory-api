using BookInventory.Application.School;
using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Finance;

/// <summary>
/// R1, R9-R11, R16: the school and vendor statements and the outstanding lists. Parties are found through the school and vendor
/// directories (school R16, vendor R15), so another company's party is not found, exactly like an unknown id. The running balance,
/// the totals and the sign of a balance follow R7; nothing here posts.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LedgerStatementService(ILedgerStatementStore store, ILedgerBalances balances, ISchoolDirectory schools, IVendorDirectory vendors)
{
    public const int MaxLines = 5000;
    public const int MaxBalanceIds = 100;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 200;

    private sealed record PartyHeader(string Code, string Name, bool IsActive);

    public async Task<LedgerStatement> SchoolStatementAsync(Guid schoolId, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var school = await schools.GetAsync(schoolId, cancellationToken) ?? throw new NotFoundException(FinanceMessages.PartyNotFound);
        return await StatementAsync(new LedgerParty(PartyType.School, schoolId), new PartyHeader(school.Code, school.Name, school.IsActive), query, cancellationToken);
    }

    public async Task<LedgerStatement> VendorStatementAsync(Guid vendorId, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var vendor = await vendors.GetAsync(vendorId, cancellationToken) ?? throw new NotFoundException(FinanceMessages.PartyNotFound);
        if (!string.IsNullOrWhiteSpace(query.Session))
            throw new ValidationFailureException(FinanceMessages.InvalidFilter);
        return await StatementAsync(new LedgerParty(PartyType.Vendor, vendorId), new PartyHeader(vendor.Code, vendor.Name, vendor.IsActive), query, cancellationToken);
    }

    public async Task<OutstandingPage> OutstandingAsync(PartyType partyType, OutstandingQuery query, CancellationToken cancellationToken)
    {
        var balances = (await store.AllTotalsAsync(partyType, cancellationToken))
            .Select(t => (t.PartyId, Balance: LedgerParty.BalanceOf(partyType, t.DebitAmount, t.CreditAmount)))
            .Where(b => b.Balance != 0)
            .ToList();
        var headers = await HeadersAsync(partyType, [.. balances.Select(b => b.PartyId)], cancellationToken);
        var search = query.Search?.Trim();
        var rows = balances
            .Where(b => headers.ContainsKey(b.PartyId))
            .Select(b => new OutstandingRow(b.PartyId, headers[b.PartyId].Code, headers[b.PartyId].Name, headers[b.PartyId].IsActive, b.Balance))
            .Where(r => string.IsNullOrEmpty(search)
                        || r.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || r.Code.Contains(search, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(r => r.Balance).ThenBy(r => r.Name)
            .ToList();

        var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
        var pageSize = Math.Clamp(query.PageSize < 1 ? DefaultPageSize : query.PageSize, 1, MaxPageSize);
        return new OutstandingPage(
            [.. rows.Skip((pageIndex - 1) * pageSize).Take(pageSize)], pageIndex, pageSize, rows.Count,
            (int)Math.Ceiling(rows.Count / (double)pageSize), rows.Sum(r => r.Balance));
    }

    /// <summary>R17: the balances of the listed parties of the session's company (another company's or unknown ids are left out).</summary>
    public async Task<IReadOnlyDictionary<Guid, decimal>> BalancesAsync(PartyType partyType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();
        if (distinct.Count > MaxBalanceIds)
            throw new ValidationFailureException(FinanceMessages.TooManyIds);

        var known = await HeadersAsync(partyType, distinct, cancellationToken);
        return await balances.BalancesAsync(partyType, [.. known.Keys], cancellationToken);
    }

    private async Task<LedgerStatement> StatementAsync(LedgerParty party, PartyHeader header, LedgerStatementQuery query, CancellationToken cancellationToken)
    {
        var filter = FilterOf(query);
        var lines = await store.LinesAsync(party, filter, MaxLines + 1, cancellationToken);
        if (lines.Count > MaxLines)
            throw new ValidationFailureException(FinanceMessages.TooManyLines);

        var before = await store.TotalsBeforeAsync(party, filter, cancellationToken);
        var opening = LedgerParty.BalanceOf(party.Type, before.DebitAmount, before.CreditAmount);
        var balance = opening;
        var rows = new List<LedgerStatementRow>(lines.Count);
        foreach (var l in lines)
        {
            balance += LedgerParty.BalanceOf(party.Type, l.DebitAmount, l.CreditAmount);
            rows.Add(new LedgerStatementRow(
                l.Date, l.DocumentType, l.DocumentId, l.DocumentNumber, l.PostingKind, l.Narration, l.Session, l.FinancialYear, l.DebitAmount, l.CreditAmount, balance));
        }

        return new LedgerStatement(
            party.Type, party.Id, header.Code, header.Name, header.IsActive, opening, rows, lines.Sum(l => l.DebitAmount), lines.Sum(l => l.CreditAmount), balance);
    }

    /// <summary>R9: exactly one kind of filter (a year, a session, or dates), or none.</summary>
    private static StatementFilter FilterOf(LedgerStatementQuery query)
    {
        var session = string.IsNullOrWhiteSpace(query.Session) ? null : query.Session.Trim();
        var hasYear = !string.IsNullOrWhiteSpace(query.FinancialYear);
        var hasDates = query.From is not null || query.To is not null;
        if ((hasYear ? 1 : 0) + (session is null ? 0 : 1) + (hasDates ? 1 : 0) > 1 || (query.From > query.To))
            throw new ValidationFailureException(FinanceMessages.InvalidFilter);

        if (hasYear)
        {
            var start = FinancialYear.StartOf(query.FinancialYear!.Trim()) ?? throw new ValidationFailureException(FinanceMessages.InvalidFilter);
            return new StatementFilter(start, start.AddYears(1), null);
        }

        return new StatementFilter(query.From, query.To?.AddDays(1), session);
    }

    private async Task<IReadOnlyDictionary<Guid, PartyHeader>> HeadersAsync(PartyType partyType, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken) =>
        partyType == PartyType.School
            ? (await schools.ReferencesAsync(ids, cancellationToken)).ToDictionary(p => p.Key, p => new PartyHeader(p.Value.Code, p.Value.Name, p.Value.IsActive))
            : (await vendors.ReferencesAsync(ids, cancellationToken)).ToDictionary(p => p.Key, p => new PartyHeader(p.Value.Code, p.Value.Name, p.Value.IsActive));
}
