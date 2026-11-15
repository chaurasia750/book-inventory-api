using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Payment;

/// <summary>A cheque of a receipt cleared (R10).</summary>
public sealed record ChequeCleared(Guid ReceiptId, Guid ModeId, Guid SchoolId, decimal Amount) : DomainEventBase;

/// <summary>A cheque of a receipt bounced (R10, R11).</summary>
public sealed record ChequeBounced(Guid ReceiptId, Guid ModeId, Guid SchoolId, decimal Amount) : DomainEventBase;

/// <summary>The advance of a receipt was applied to invoices (R13).</summary>
public sealed record AdvanceAdjusted(Guid ReceiptId, Guid SchoolId, decimal Amount) : DomainEventBase;

/// <summary>A receipt was cancelled (R14).</summary>
public sealed record ReceiptCancelled(Guid ReceiptId, Guid SchoolId, string Number, decimal TotalAmount) : DomainEventBase;

/// <summary>A school payment was recorded (R4-R7).</summary>
public sealed record ReceiptRecorded(Guid ReceiptId, Guid SchoolId, string Number, decimal TotalAmount) : DomainEventBase;
