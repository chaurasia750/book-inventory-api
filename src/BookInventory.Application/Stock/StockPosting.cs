using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Stock;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Stock;

/// <summary>
/// R2, R9, R11, R12: the posting capability of <see cref="IStockPosting"/>. Lines of the same item, stock type and kind are posted as
/// one entry (the unique posting index counts each once); lines of items that are not stock managed are skipped. Every entry goes
/// through <see cref="ItemStock"/>, which never refuses a low quantity.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IStockPosting))]
public sealed class StockPosting(IStockStore store, StockCards cards, StockManagedItems items) : IStockPosting
{
    public async Task<StockPostingResult> PostAsync(StockDocumentPosting posting, CancellationToken cancellationToken = default)
    {
        var document = posting.Document;
        if ((await store.DocumentEntriesAsync(document.DocumentType, document.DocumentId, cancellationToken)).Count > 0)
            throw new ConflictException(StockMessages.DocumentAlreadyPosted);

        var managed = await items.StockManagedOfAsync([.. posting.Lines.Select(l => l.ItemId).Distinct()], cancellationToken);
        var lines = Merged(posting.Lines.Where(l => managed.Contains(l.ItemId)));
        var stocks = await cards.ForItemsAsync([.. lines.Select(l => l.ItemId).Distinct()], cancellationToken);

        foreach (var line in lines)
        {
            stocks[line.ItemId].Post(new StockMovement
            {
                StockType = line.StockType,
                Change = line.Change,
                MovementDate = posting.DocumentDate,
                Document = document
            });
        }

        return ResultOf(lines.Count, lines.Select(l => new StockKey(l.ItemId, l.StockType)), stocks);
    }

    public async Task<StockPostingResult> ReverseAsync(StockDocument document, DateOnly reversalDate, CancellationToken cancellationToken = default)
    {
        var posted = await store.DocumentEntriesAsync(document.DocumentType, document.DocumentId, cancellationToken);
        if (posted.Count == 0)
            throw new ConflictException(StockMessages.DocumentNotPosted);
        if (posted.Any(e => e.MovementKind == MovementKind.Reversal))
            throw new ConflictException(StockMessages.DocumentAlreadyReversed);

        var stocks = await cards.ForItemsAsync([.. posted.Select(e => e.ItemId).Distinct()], cancellationToken);
        foreach (var entry in posted)
            stocks[entry.ItemId].Reverse(entry.StockType, entry.Change, document, reversalDate);

        return ResultOf(posted.Count, posted.Select(e => new StockKey(e.ItemId, e.StockType)), stocks);
    }

    /// <summary>Lines of the same item, stock type and kind become one change with their net quantity; a net of zero posts nothing.</summary>
    private static List<StockPostingLine> Merged(IEnumerable<StockPostingLine> lines) =>
    [
        .. lines
            .GroupBy(l => (l.ItemId, l.StockType, l.Change.Kind))
            .Select(g => (g.Key, Net: g.Sum(l => l.Change.Net)))
            .Where(g => g.Net != 0)
            .Select(g => new StockPostingLine(
                g.Key.ItemId,
                g.Key.StockType,
                g.Net > 0 ? StockChange.In(g.Key.Kind, g.Net) : StockChange.Out(g.Key.Kind, -g.Net)))
    ];

    private static StockPostingResult ResultOf(int posted, IEnumerable<StockKey> touched, IReadOnlyDictionary<Guid, ItemStock> stocks) =>
        new(posted, [.. touched.Where(k => stocks[k.ItemId].QuantityOf(k.StockType) <= 0).Select(k => k.ItemId).Distinct()]);
}
