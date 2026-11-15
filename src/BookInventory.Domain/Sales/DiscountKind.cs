namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R3, R7 (BR-DIS-02): a discount is none, a percentage or a rupee amount.</summary>
public enum DiscountKind
{
    None,
    Percent,
    Amount
}
