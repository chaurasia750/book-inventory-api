using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>The debit and credit sums of one party.</summary>
public sealed record PartyTotals(Guid PartyId, decimal DebitAmount, decimal CreditAmount);

/// <summary>
/// The persistence port of the <c>finance</c> module. Every read is already limited to the session's company by the platform filter
/// (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ILedgerStore
{
    Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default);

    /// <summary>R5: the transactions of a document (its posting and its reversal, if any) with their lines, not tracked.</summary>
    Task<IReadOnlyList<LedgerTransaction>> OfDocumentAsync(string documentType, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>R7: the debit and credit sums of these parties of one type; a party with no line is left out.</summary>
    Task<IReadOnlyList<PartyTotals>> TotalsAsync(PartyType partyType, IReadOnlyCollection<Guid> partyIds, CancellationToken cancellationToken = default);
}
