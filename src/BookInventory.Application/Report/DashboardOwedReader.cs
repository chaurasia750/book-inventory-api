using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Finance;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Report;

/// <summary>R9: the owed card of the dashboard (the Owner's), read through the finance outstanding list (finance R11): today's balances, whatever the session.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DashboardOwedReader(LedgerStatementService statements)
{
    /// <summary>R9 (Q3): the five largest.</summary>
    private const int Largest = 5;

    private const int PageSize = 200;

    public async Task<OwedPart> ReadAsync(CancellationToken cancellationToken)
    {
        var schools = await AllAsync(PartyType.School, cancellationToken);
        var vendors = await AllAsync(PartyType.Vendor, cancellationToken);
        return new OwedPart(
            GroupOf(schools.Where(r => r.Balance > 0m).OrderByDescending(r => r.Balance)),
            GroupOf(schools.Where(r => r.Balance < 0m).OrderBy(r => r.Balance), absolute: true),
            GroupOf(vendors.Where(r => r.Balance > 0m).OrderByDescending(r => r.Balance)));
    }

    /// <summary>One group: how many, their total and the largest five; an advance (a negative school balance) is shown as a positive amount.</summary>
    private static OwedGroup GroupOf(IEnumerable<OutstandingRow> rows, bool absolute = false)
    {
        var list = rows.ToList();
        decimal Amount(OutstandingRow r) => absolute ? -r.Balance : r.Balance;
        return new OwedGroup(list.Count, list.Sum(Amount), [.. list.Take(Largest).Select(r => new OwedRow(r.PartyId, r.Code, r.Name, Amount(r)))]);
    }

    /// <summary>Every page of the outstanding list of one kind of party (the list is paged at 200).</summary>
    private async Task<IReadOnlyList<OutstandingRow>> AllAsync(PartyType type, CancellationToken cancellationToken)
    {
        var rows = new List<OutstandingRow>();
        var page = 1;
        OutstandingPage current;
        do
        {
            current = await statements.OutstandingAsync(type, new OutstandingQuery { PageIndex = page++, PageSize = PageSize }, cancellationToken);
            rows.AddRange(current.Items);
        }
        while (page <= current.TotalPages);
        return rows;
    }
}
