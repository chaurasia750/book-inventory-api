using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Finance;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Finance;

/// <summary>R7: balances summed from the lines with the sign rule of <see cref="LedgerParty.BalanceOf"/>.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ILedgerBalances))]
public sealed class LedgerBalances(ILedgerStore store) : ILedgerBalances
{
    public async Task<decimal> BalanceOfAsync(LedgerParty party, CancellationToken cancellationToken = default) =>
        (await BalancesAsync(party.Type, [party.Id], cancellationToken))[party.Id];

    public async Task<IReadOnlyDictionary<Guid, decimal>> BalancesAsync(
        PartyType partyType, IReadOnlyCollection<Guid> partyIds, CancellationToken cancellationToken = default)
    {
        var totals = (await store.TotalsAsync(partyType, partyIds, cancellationToken)).ToDictionary(t => t.PartyId);
        return partyIds.Distinct().ToDictionary(
            id => id,
            id => totals.TryGetValue(id, out var t) ? LedgerParty.BalanceOf(partyType, t.DebitAmount, t.CreditAmount) : 0m);
    }
}
