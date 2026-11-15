namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R7: the computed lines and totals of an order or an invoice.</summary>
public sealed record SaleAmounts(IReadOnlyList<SaleLineAmounts> Lines, SaleTotals Totals);
