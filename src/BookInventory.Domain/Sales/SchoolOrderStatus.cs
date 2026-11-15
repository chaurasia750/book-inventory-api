namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R6: a draft, a confirmed order, or a closed one (all invoiced, or closed by the Owner).</summary>
public enum SchoolOrderStatus
{
    Draft,
    Confirmed,
    Closed
}
