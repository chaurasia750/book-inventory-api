namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R3, R13: what a line keeps of the item.</summary>
public sealed record SaleItem(Guid Id, string Code, string Name, string? HsnCode, decimal GstRatePercent);
