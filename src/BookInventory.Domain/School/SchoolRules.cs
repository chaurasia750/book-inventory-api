using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.School;

/// <summary>
/// The validation and normalisation of school values (R2, R7, R13), written once. Mobile and email use the same rules as
/// <c>iam</c> (<see cref="IdentityNormalizer"/>, R13), so one number has one stored form across the system.
/// </summary>
internal static partial class SchoolRules
{
    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < School.NameMinLength or > School.NameMaxLength)
            throw new ValidationFailureException(SchoolMessages.InvalidName);
        return cleaned;
    }

    public static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(SchoolMessages.InvalidField);
    }

    public static string? Mobile(string? value) => ContactRules.Mobile(value, SchoolMessages.InvalidMobile);

    public static string? Email(string? value) => ContactRules.Email(value, SchoolMessages.InvalidEmail);

    /// <summary>R21: 15 characters in the GST shape, stored in upper case; empty is none.</summary>
    public static string? Gstin(string? value) => ContactRules.Gstin(value, SchoolMessages.InvalidGstin);

    /// <summary>R7: zero or more, two decimals (an amount with more decimals is refused, never rounded silently).</summary>
    public static decimal OpeningBalance(decimal amount)
    {
        if (amount < 0m || decimal.Round(amount, 2) != amount)
            throw new ValidationFailureException(SchoolMessages.InvalidOpeningBalance);
        return amount;
    }

    /// <summary>R6: a class name, trimmed and with single spaces, 1-50 characters.</summary>
    public static string RequireClassName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < 1 or > SchoolClass.NameMaxLength)
            throw new ValidationFailureException(SchoolMessages.InvalidClassName);
        return cleaned;
    }

    public static string Normalize(string value) => value.ToLowerInvariant();

    /// <summary>R6, R16: the form a class name is compared in (trimmed, single spaces, lower case), or null for an empty text.</summary>
    public static string? ClassKeyOf(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        return cleaned.Length == 0 ? null : Normalize(cleaned);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
