namespace BookInventory.Domain.Item;

/// <summary>R3: what kind of thing an item is (BRD FR-ITM-01). A fixed list for now (Q1).</summary>
public enum ItemCategory
{
    Book,

    Copy,

    Pen,

    Pencil,

    Bag,

    Tie,

    Other
}

/// <summary>R4/BR-STK: the stock-managed default of a category (FR-ITM-03): Book and Copy are counted, the others are not.</summary>
public static class ItemCategoryRules
{
    public static bool IsStockManagedByDefault(ItemCategory category) => category is ItemCategory.Book or ItemCategory.Copy;
}
