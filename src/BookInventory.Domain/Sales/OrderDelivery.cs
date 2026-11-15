namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R8: what an invoice takes from an order: its lines and the total discount this invoice applies.</summary>
public sealed record OrderDelivery(IReadOnlyList<DeliveryLine> Lines, Discount TotalDiscount);
