namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R3, R20, R21: one line as the order receives it: the class it is for (none: the school's general need) and, when it
/// edits a line of a confirmed order, that line's id.
/// </summary>
public sealed record OrderLineInput(SaleItem Item, SalePrice Price, int Quantity, Discount ItemDiscount, string? ClassName = null, Guid? LineId = null);
