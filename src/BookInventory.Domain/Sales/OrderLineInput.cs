namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R3: one line as the order receives it.</summary>
public sealed record OrderLineInput(SaleItem Item, SalePrice Price, int Quantity, Discount ItemDiscount);
