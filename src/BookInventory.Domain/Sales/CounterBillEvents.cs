using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Sales;

/// <summary>A counter bill was made (spec sales R42): ids and values only.</summary>
public sealed record CounterBillMade(Guid CounterBillId, Guid SchoolId, string Number, decimal GrandTotal) : DomainEventBase;

/// <summary>The Owner cancelled a counter bill (R47).</summary>
public sealed record CounterBillCancelled(Guid CounterBillId, Guid SchoolId, string Number) : DomainEventBase;
