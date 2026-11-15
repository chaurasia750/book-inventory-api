using BookInventory.Domain.Item;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R43: one counter bill line as the use case resolved it: the item (with its category, for the books count of R50),
/// the chosen price version (the price of the line is its MRP, Q25), the quantity and the item discount.
/// </summary>
public sealed record CounterLineInput(SaleItem Item, ItemCategory Category, SalePrice Price, int Quantity, Discount ItemDiscount);
