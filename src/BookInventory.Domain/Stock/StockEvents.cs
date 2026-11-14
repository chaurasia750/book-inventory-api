using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock section 7: an item's stock moved: the item, the stock type, the change (kind, quantity in, quantity out) and the
/// quantity after it. Raised by <see cref="ItemStock"/>; it carries ids and values, never entities.
/// </summary>
public sealed record StockMoved(Guid ItemId, StockType StockType, StockChange Change, int NewQuantity) : DomainEventBase;
