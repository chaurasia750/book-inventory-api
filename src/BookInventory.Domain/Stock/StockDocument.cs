namespace BookInventory.Domain.Stock;

/// <summary>
/// Spec stock R4, R11: the document a movement comes from (for example an invoice): its type, id and number. Opening and
/// adjustment entries have none.
/// </summary>
public sealed record StockDocument(string DocumentType, Guid DocumentId, string DocumentNumber)
{
    public const int TypeMaxLength = 50;
    public const int NumberMaxLength = 50;
}
