namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R7: the computed amounts of one line, in the order of the rule.</summary>
public sealed record SaleLineAmounts(
    decimal Gross,
    decimal ItemDiscount,
    decimal LineAmount,
    decimal TotalDiscountShare,
    decimal Taxable,
    decimal Cgst,
    decimal Sgst,
    decimal Igst);
