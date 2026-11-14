namespace BookInventory.Domain.Item;

/// <summary>R8: an item is usable or switched off with its data kept (BRD FR-ITM-08). Never deleted.</summary>
public enum ItemStatus
{
    Active,

    Inactive
}
