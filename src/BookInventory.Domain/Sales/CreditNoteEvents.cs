using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Sales;

/// <summary>A credit note was made (spec sales R53): ids and values only.</summary>
public sealed record CreditNoteMade(Guid CreditNoteId, Guid SchoolId, string Number, decimal GrandTotal) : DomainEventBase;

/// <summary>The Owner cancelled a credit note (R58).</summary>
public sealed record CreditNoteCancelled(Guid CreditNoteId, Guid SchoolId, string Number) : DomainEventBase;

/// <summary>A refund was recorded (spec sales R64): ids and values only.</summary>
public sealed record RefundRecorded(Guid RefundId, Guid SchoolId, string Number, decimal Amount) : DomainEventBase;

/// <summary>The Owner cancelled a refund (R67).</summary>
public sealed record RefundCancelled(Guid RefundId, Guid SchoolId, string Number) : DomainEventBase;
