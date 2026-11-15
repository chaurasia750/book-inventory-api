using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Payment;

/// <summary>A cheque of a receipt cleared (R10).</summary>
public sealed record ChequeCleared(Guid ReceiptId, Guid ModeId, Guid SchoolId, decimal Amount) : DomainEventBase;

/// <summary>A cheque of a receipt bounced (R10, R11).</summary>
public sealed record ChequeBounced(Guid ReceiptId, Guid ModeId, Guid SchoolId, decimal Amount) : DomainEventBase;

/// <summary>A school payment was recorded (R4-R7).</summary>
public sealed record ReceiptRecorded(Guid ReceiptId, Guid SchoolId, string Number, decimal TotalAmount) : DomainEventBase;
