namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R3, R13 (BR-PRC-03): what a line keeps of the chosen price version: its id, name, MRP and rate (the sale price).</summary>
public sealed record SalePrice(Guid VersionId, string VersionName, decimal Mrp, decimal Rate);
