using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Customer;

/// <summary>Domain events raised by <see cref="Customer"/> (spec customer section 7). They carry ids and values, never entities.</summary>
public abstract record CustomerEvent(Guid CustomerId) : DomainEventBase;

/// <summary>A customer was created with its generated code (R10).</summary>
public sealed record CustomerCreated(Guid CustomerId, string Code, string Name) : CustomerEvent(CustomerId);

/// <summary>A customer was switched off (R5); it is no longer suggested.</summary>
public sealed record CustomerDeactivated(Guid CustomerId) : CustomerEvent(CustomerId);

/// <summary>A deactivated customer is usable again (R5).</summary>
public sealed record CustomerReactivated(Guid CustomerId) : CustomerEvent(CustomerId);
