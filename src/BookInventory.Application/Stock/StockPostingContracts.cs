using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>R11: one line of a document's stock movement: the item, the stock type and the change (kind, quantity in or out).</summary>
public sealed record StockPostingLine(Guid ItemId, StockType StockType, StockChange Change);

/// <summary>R11, R12: a document's lines, posted as of the document's date.</summary>
public sealed record StockDocumentPosting(StockDocument Document, DateOnly DocumentDate, IReadOnlyList<StockPostingLine> Lines);

/// <summary>R9, R11: how many entries were posted and which items have a stock at zero or below after them (the document's warning).</summary>
public sealed record StockPostingResult(int EntriesPosted, IReadOnlyList<Guid> ItemsAtOrBelowZero);
