namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R7: what the amounts rule needs of one line.</summary>
public sealed record SaleLineInput(decimal Rate, int Quantity, Discount ItemDiscount, decimal GstRatePercent);
