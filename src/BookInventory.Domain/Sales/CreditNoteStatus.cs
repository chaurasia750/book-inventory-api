namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R58: a credit note is active, or cancelled (it keeps its number).</summary>
public enum CreditNoteStatus
{
    Active,
    Cancelled
}
