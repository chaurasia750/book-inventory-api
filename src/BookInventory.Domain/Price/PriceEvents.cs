using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Price;

/// <summary>Domain events raised by <see cref="PriceVersion"/> (spec price section 7). They carry ids and values, never entities.</summary>
public abstract record PriceVersionEvent(Guid PriceVersionId, Guid ItemId) : DomainEventBase;

/// <summary>A price version was added to an item (R3).</summary>
public sealed record PriceVersionAdded(Guid PriceVersionId, Guid ItemId, string VersionName) : PriceVersionEvent(PriceVersionId, ItemId);

/// <summary>The values of a price version were changed (R3, R13).</summary>
public sealed record PriceVersionChanged(Guid PriceVersionId, Guid ItemId, string VersionName) : PriceVersionEvent(PriceVersionId, ItemId);
