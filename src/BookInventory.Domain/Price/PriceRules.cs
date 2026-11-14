using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Price;

/// <summary>The validation and normalisation of price values (R2, R17), written once.</summary>
internal static partial class PriceRules
{
    /// <summary>The largest value a <c>numeric(18,2)</c> column holds.</summary>
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < PriceVersion.NameMinLength or > PriceVersion.NameMaxLength)
            throw new ValidationFailureException(PriceMessages.InvalidName);
        return cleaned;
    }

    /// <summary>R2: the form two version names are compared in: single spaces, lower case.</summary>
    public static string NormalizeName(string? name) => SpaceRun().Replace(name?.Trim() ?? string.Empty, " ").ToLowerInvariant();

    /// <summary>R2, R17: zero or more, at most two decimals (never rounded silently) and within the column.</summary>
    public static decimal Amount(decimal amount)
    {
        if (amount < 0m || amount > MaxAmount || decimal.Round(amount, 2) != amount)
            throw new ValidationFailureException(PriceMessages.InvalidAmount);
        return amount;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
