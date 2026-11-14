using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.School;

/// <summary>Domain events raised by <see cref="School"/> (spec school section 7). They carry ids and values, never entities.</summary>
public abstract record SchoolEvent(Guid SchoolId) : DomainEventBase;

/// <summary>A school was created with its generated code (R15).</summary>
public sealed record SchoolCreated(Guid SchoolId, string Code, string Name) : SchoolEvent(SchoolId);

/// <summary>A school was switched off (R8); it cannot be chosen for a new document.</summary>
public sealed record SchoolDeactivated(Guid SchoolId) : SchoolEvent(SchoolId);

/// <summary>A deactivated school is usable again (R8).</summary>
public sealed record SchoolReactivated(Guid SchoolId) : SchoolEvent(SchoolId);

/// <summary>R17: the opening balance was set or changed (a new school counts from zero); the ledger posts the difference.</summary>
public sealed record SchoolOpeningBalanceChanged(Guid SchoolId, string Code, decimal OldAmount, decimal NewAmount) : SchoolEvent(SchoolId);
