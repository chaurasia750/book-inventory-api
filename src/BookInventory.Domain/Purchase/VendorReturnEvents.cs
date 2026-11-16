using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Purchase;

/// <summary>A vendor return was made (spec purchase R2): ids and values only.</summary>
public sealed record VendorReturnMade(Guid VendorReturnId, Guid VendorId, string Number, decimal GrandTotal) : DomainEventBase;

/// <summary>The Owner cancelled a vendor return (R8).</summary>
public sealed record VendorReturnCancelled(Guid VendorReturnId, Guid VendorId, string Number) : DomainEventBase;
