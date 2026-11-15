using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Payment;

/// <summary>A school payment was recorded (R4-R7).</summary>
public sealed record ReceiptRecorded(Guid ReceiptId, Guid SchoolId, string Number, decimal TotalAmount) : DomainEventBase;
