namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R3, R4, R12: what a document posts: the document, its date, its session (if any), a narration and the lines.</summary>
public sealed record LedgerPosting
{
    public required LedgerDocument Document { get; init; }

    public required DateOnly TransactionDate { get; init; }

    public string? Session { get; init; }

    public string? Narration { get; init; }

    public required IReadOnlyList<PostingLine> Lines { get; init; }
}
