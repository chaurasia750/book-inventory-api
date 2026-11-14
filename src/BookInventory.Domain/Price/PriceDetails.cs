namespace BookInventory.Domain.Price;

/// <summary>R2: the values of a price version as they arrive; <see cref="PriceVersion"/> validates and normalises every value.</summary>
public sealed record PriceDetails(string VersionName, DateOnly EffectiveFrom, decimal MrpAmount, decimal SaleAmount, decimal PurchaseAmount);
