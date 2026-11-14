using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>The two small text rules several roots of the <c>iam</c> module share, written once.</summary>
internal static class TextRules
{
    /// <summary>Client-supplied text is cut to the column length instead of failing a login; empty becomes null.</summary>
    public static string? Truncate(string? value, int maxLength) =>
        value is { Length: > 0 } ? value[..Math.Min(value.Length, maxLength)] : null;

    /// <summary>A token hash is mandatory; the failure carries the code of the root that asked.</summary>
    public static string RequireHash(string? value, string code)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationFailureException(code);
        return value;
    }
}
