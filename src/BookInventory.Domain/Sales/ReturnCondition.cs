namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R54, R57 (FR-RET-04): a returned line is good (back to Main stock) or damaged (to Damaged stock).</summary>
public enum ReturnCondition
{
    Good,
    Damaged
}
