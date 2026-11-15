namespace BookInventory.Application.Item;

/// <summary>R21: what one optional cell of the file says: nothing (keep the current value), <c>-</c> (clear it) or a value.</summary>
public readonly record struct ImportCell(bool IsClear, string? Value)
{
    public const string ClearMark = "-";

    public static ImportCell Keep { get; } = new(false, null);

    public bool IsGiven => IsClear || Value is not null;

    public static ImportCell Of(string text) =>
        text.Length == 0 ? Keep : text == ClearMark ? new ImportCell(true, null) : new ImportCell(false, text);

    /// <summary>The value an item gets: the current one when the cell is empty, none when it clears, else the given text.</summary>
    public string? ApplyTo(string? current) => IsClear ? null : Value ?? current;
}
