namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R3, R5: the document a transaction comes from: its type, id and number.</summary>
public sealed record LedgerDocument(string DocumentType, Guid DocumentId, string DocumentNumber)
{
    public const int TypeMaxLength = 50;
    public const int NumberMaxLength = 50;
}
