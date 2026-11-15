namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R5: a document's own posting, or the reversal of it when the document is cancelled.</summary>
public enum PostingKind
{
    Post,
    Reversal
}
