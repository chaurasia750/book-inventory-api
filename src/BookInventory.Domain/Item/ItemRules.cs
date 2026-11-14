using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Item;

/// <summary>The validation and normalisation of item values (R2, R6, R7), written once.</summary>
internal static partial class ItemRules
{
    /// <summary>R7: the GST rates an item may carry, in percent (Q3).</summary>
    public static readonly IReadOnlyList<decimal> GstRates = [0m, 5m, 12m, 18m, 28m];

    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < Item.NameMinLength or > Item.NameMaxLength)
            throw new ValidationFailureException(ItemMessages.InvalidName);
        return cleaned;
    }

    public static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(ItemMessages.InvalidField);
    }

    /// <summary>R2: 4 to 8 digits, or none.</summary>
    public static string? Hsn(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return HsnShape().IsMatch(trimmed) ? trimmed : throw new ValidationFailureException(ItemMessages.InvalidHsn);
    }

    /// <summary>R7: one of the rates of <see cref="GstRates"/>; a missing rate is 0.</summary>
    public static decimal GstRate(decimal? percent)
    {
        var rate = percent ?? 0m;
        return GstRates.Contains(rate) ? rate : throw new ValidationFailureException(ItemMessages.InvalidGstRate);
    }

    /// <summary>R6: 4-30 letters, digits and hyphens, stored in upper case (Q5).</summary>
    public static string Barcode(string? value)
    {
        var upper = value?.Trim().ToUpperInvariant() ?? string.Empty;
        return BarcodeShape().IsMatch(upper) ? upper : throw new ValidationFailureException(ItemMessages.InvalidBarcode);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();

    [GeneratedRegex(@"^\d{4,8}$")]
    private static partial Regex HsnShape();

    [GeneratedRegex(@"^[A-Z0-9-]{4,30}$")]
    private static partial Regex BarcodeShape();
}
