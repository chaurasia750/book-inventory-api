namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R8: one order line delivered on an invoice: its snapshot, the quantity now and the item discount this invoice applies.</summary>
public sealed record DeliveryLine(Guid OrderLineId, SaleItem Item, SalePrice Price, int Quantity, Discount ItemDiscount);
