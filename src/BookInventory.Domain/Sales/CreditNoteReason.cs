namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R56 (FR-RET-05): why the school gives the books back; once per credit note.</summary>
public enum CreditNoteReason
{
    SessionEnd,
    Damaged,
    WrongBook,
    Other
}
