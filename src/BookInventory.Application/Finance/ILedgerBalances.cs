using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R7: the balance of parties, always summed from their ledger lines (a school's or customer's: debit minus credit;
/// a vendor's: credit minus debit). Later modules (the invoice's credit check, the counter's credit-limit warning) read it here.
/// </summary>
public interface ILedgerBalances
{
    Task<decimal> BalanceOfAsync(LedgerParty party, CancellationToken cancellationToken = default);

    /// <summary>The balance of each of these parties of one type; a party with no line has 0.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> BalancesAsync(PartyType partyType, IReadOnlyCollection<Guid> partyIds, CancellationToken cancellationToken = default);
}
