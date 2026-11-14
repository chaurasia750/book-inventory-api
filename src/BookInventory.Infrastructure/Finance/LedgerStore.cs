using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Finance;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Finance;

/// <summary>
/// The data side of the ledger (spec finance R5, R7). Lines are reached through their transaction, never through a repository of
/// their own. Every query is already limited to the session's company by the platform filter.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ILedgerStore))]
public sealed class LedgerStore(IUnitOfWork unitOfWork) : ILedgerStore
{
    private readonly IRepository<LedgerTransaction> _transactions = unitOfWork.Repository<LedgerTransaction>();

    public Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken = default) =>
        _transactions.AddAsync(transaction, cancellationToken);

    public async Task<IReadOnlyList<LedgerTransaction>> OfDocumentAsync(string documentType, Guid documentId, CancellationToken cancellationToken = default) =>
        await _transactions.TableNoTracking
            .Include(t => t.Lines)
            .Where(t => t.DocumentType == documentType && t.DocumentId == documentId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<PartyTotals>> TotalsAsync(
        PartyType partyType, IReadOnlyCollection<Guid> partyIds, CancellationToken cancellationToken = default) =>
        partyIds.Count == 0
            ? []
            : await _transactions.TableNoTracking
                .SelectMany(t => t.Lines)
                .Where(l => l.PartyType == partyType && l.PartyId != null && partyIds.Contains(l.PartyId.Value))
                .GroupBy(l => l.PartyId!.Value)
                .Select(g => new PartyTotals(g.Key, g.Sum(l => l.DebitAmount), g.Sum(l => l.CreditAmount)))
                .ToListAsync(cancellationToken);
}
