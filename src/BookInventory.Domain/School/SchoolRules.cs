using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;

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

    public static string? Mobile(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeMobile(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidMobile(normalized) ? normalized : throw new ValidationFailureException(SchoolMessages.InvalidMobile);
    }

    public static string? Email(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeEmail(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidEmail(normalized) ? normalized : throw new ValidationFailureException(SchoolMessages.InvalidEmail);
    }

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

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
