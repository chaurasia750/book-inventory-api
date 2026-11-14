using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Finance;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Finance;

/// <summary>
/// R5, R15: the posting capability. Every line rule is the aggregate's; this class checks what needs the stored history (was the
/// document posted, was it reversed) and adds the transaction to the request's unit of work. The unique posting index is the real
/// guard against two parallel postings of one document.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ILedgerPosting))]
public sealed class LedgerPostingService(ILedgerStore store) : ILedgerPosting
{
    public async Task<Guid> PostAsync(LedgerPosting posting, CancellationToken cancellationToken = default)
    {
        var existing = await store.OfDocumentAsync(posting.Document.DocumentType.Trim(), posting.Document.DocumentId, cancellationToken);
        if (existing.Count > 0)
            throw new ConflictException(FinanceMessages.AlreadyPosted);

        var transaction = LedgerTransaction.Post(posting);
        await store.AddAsync(transaction, cancellationToken);
        return transaction.Id;
    }

    public async Task<Guid> ReverseAsync(LedgerDocument document, DateOnly reversalDate, CancellationToken cancellationToken = default)
    {
        var existing = await store.OfDocumentAsync(document.DocumentType.Trim(), document.DocumentId, cancellationToken);
        var posted = existing.FirstOrDefault(t => t.PostingKind == PostingKind.Post)
                     ?? throw new ConflictException(FinanceMessages.NotPosted);
        if (existing.Any(t => t.PostingKind == PostingKind.Reversal))
            throw new ConflictException(FinanceMessages.AlreadyReversed);

        var reversal = posted.Reverse(reversalDate);
        await store.AddAsync(reversal, cancellationToken);
        return reversal.Id;
    }
}
