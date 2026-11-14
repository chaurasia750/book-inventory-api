using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Vendor;

/// <summary>Domain events raised by <see cref="Vendor"/> (spec vendor section 7). They carry ids and values, never entities.</summary>
public abstract record VendorEvent(Guid VendorId) : DomainEventBase;

/// <summary>A vendor was created with its generated code (R11).</summary>
public sealed record VendorCreated(Guid VendorId, string Code, string Name) : VendorEvent(VendorId);

/// <summary>A vendor was switched off (R7); it cannot be chosen for a new document.</summary>
public sealed record VendorDeactivated(Guid VendorId) : VendorEvent(VendorId);

/// <summary>A deactivated vendor is usable again (R7).</summary>
public sealed record VendorReactivated(Guid VendorId) : VendorEvent(VendorId);

/// <summary>R16: the opening balance was set or changed (a new vendor counts from zero); the ledger posts the difference.</summary>
public sealed record VendorOpeningBalanceChanged(Guid VendorId, string Code, decimal OldAmount, decimal NewAmount) : VendorEvent(VendorId);
