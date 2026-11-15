using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Sales;

/// <summary>A school order was confirmed (R6).</summary>
public sealed record SchoolOrderConfirmed(Guid OrderId, Guid SchoolId, string Number) : DomainEventBase;

/// <summary>A school order was closed: all invoiced, or closed by the Owner with a reason (R6).</summary>
public sealed record SchoolOrderClosed(Guid OrderId, Guid SchoolId, string Number) : DomainEventBase;

/// <summary>An invoice was made (R8-R11).</summary>
public sealed record InvoiceIssued(Guid InvoiceId, Guid SchoolId, string Number, decimal GrandTotal) : DomainEventBase;

/// <summary>An invoice was cancelled (R12).</summary>
public sealed record InvoiceCancelled(Guid InvoiceId, Guid SchoolId, string Number) : DomainEventBase;
