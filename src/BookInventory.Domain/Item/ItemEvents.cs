using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Item;

/// <summary>Domain events raised by <see cref="Item"/> (spec item section 7). They carry ids and values, never entities.</summary>
public abstract record ItemEvent(Guid ItemId) : DomainEventBase;

/// <summary>An item was created with its generated code (R10).</summary>
public sealed record ItemCreated(Guid ItemId, string Code, string Name) : ItemEvent(ItemId);

/// <summary>An item was switched off (R8); it cannot be chosen for a new document.</summary>
public sealed record ItemDeactivated(Guid ItemId) : ItemEvent(ItemId);

/// <summary>A deactivated item is usable again (R8).</summary>
public sealed record ItemReactivated(Guid ItemId) : ItemEvent(ItemId);
