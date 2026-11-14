using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>
/// Spec finance R5, R15, D2: the one way other modules (Invoice, Payment, Return, Purchase, Sample bill) write the ledger, inside
/// their own request transaction. A document type and id is posted once; a cancel posts its exact opposite once. No other module
/// writes ledger entries or changes a balance any other way.
/// </summary>
public interface ILedgerPosting
{
    /// <summary>Posts a document's balanced lines; <c>409 finance.already_posted</c> when the document was posted before. Returns the transaction id.</summary>
    Task<Guid> PostAsync(LedgerPosting posting, CancellationToken cancellationToken = default);

    /// <summary>The cancel of a posted document, dated <paramref name="reversalDate"/>; <c>409</c> when never posted or already reversed.</summary>
    Task<Guid> ReverseAsync(LedgerDocument document, DateOnly reversalDate, CancellationToken cancellationToken = default);
}
