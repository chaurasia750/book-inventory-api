namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R2, R13: what an order and its invoices keep of the school.</summary>
public sealed record SaleSchool(Guid Id, string Code, string Name, string? State);
