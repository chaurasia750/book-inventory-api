namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R8: one order line delivered on an invoice: its snapshot, the quantity now and the item discount this invoice applies and the class of the order line (R21).</summary>
public sealed record DeliveryLine(Guid OrderLineId, SaleItem Item, SalePrice Price, int Quantity, Discount ItemDiscount, string? ClassName = null);
