using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Company;

/// <summary>The validation and normalisation of company values (R3, R7), written once so no caller repeats a rule.</summary>
internal static partial class CompanyRules
{
    public static string NormalizeName(string? name) =>
        SpaceRun().Replace(name?.Trim() ?? string.Empty, " ").ToLowerInvariant();

    /// <summary>R3: the display name, trimmed and with single spaces; 2-150 characters.</summary>
    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < Company.NameMinLength or > Company.NameMaxLength)
            throw new ValidationFailureException(CompanyMessages.InvalidName);
        return cleaned;
    }

    /// <summary>An optional text: trimmed, empty becomes null, longer than <paramref name="maxLength"/> is refused.</summary>
    public static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(CompanyMessages.InvalidField);
    }

    /// <summary>R7: the reason of a deactivation, 5-500 characters.</summary>
    public static string RequireReason(string? reason)
    {
        var trimmed = reason?.Trim();
        if (trimmed is not { Length: >= Company.ReasonMinLength and <= Company.ReasonMaxLength })
            throw new ValidationFailureException(CompanyMessages.ReasonRequired);
        return trimmed;
    }

    /// <summary>R7: the optional note of a reactivation; empty is allowed, longer than 500 characters is not.</summary>
    public static string OptionalReason(string? note)
    {
        var trimmed = note?.Trim() ?? string.Empty;
        return trimmed.Length <= Company.ReasonMaxLength ? trimmed : throw new ValidationFailureException(CompanyMessages.ReasonRequired);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
