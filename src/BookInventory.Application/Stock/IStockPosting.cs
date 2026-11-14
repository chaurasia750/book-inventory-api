using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>
/// Spec stock R11, D2: the one way other modules (Purchase, School Invoice, Counter Sale, Return, Sample) move stock. It runs
/// inside the caller's request transaction, skips items that are not stock managed (R2), never refuses for low stock (R9), refuses
/// a document that was already posted and reverses exactly what a document posted. No other module writes stock entries.
/// </summary>
public interface IStockPosting
{
    /// <summary>Posts a document's lines; <c>409 stock.document_already_posted</c> when this document type and id was posted before.</summary>
    Task<StockPostingResult> PostAsync(StockDocumentPosting posting, CancellationToken cancellationToken = default);

    /// <summary>A cancel: one <see cref="MovementKind.Reversal"/> entry for each entry the document posted, dated <paramref name="reversalDate"/>.</summary>
    Task<StockPostingResult> ReverseAsync(StockDocument document, DateOnly reversalDate, CancellationToken cancellationToken = default);
}
