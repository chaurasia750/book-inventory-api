using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// Turns the three unique identity values of a user (R1) into their stored form, so uniqueness is a plain database
/// index instead of a case-insensitive comparison. Username and email are lower-cased and trimmed, mobile keeps digits
/// only. The normalised value is what is stored, compared and logged; the original casing is only kept for
/// <c>FullName</c>.
/// </summary>
public static partial class IdentityNormalizer
{
    public const int UsernameMinLength = 3;
    public const int UsernameMaxLength = 32;
    public const int EmailMaxLength = 254;
    public const int MobileMinLength = 10;
    public const int MobileMaxLength = 15;
    public const int FullNameMaxLength = 200;

    /// <summary>Normalised form of a username, or <c>null</c> when the input is missing.</summary>
    public static string? NormalizeUserName(string? value) => Clean(value);

    public static string? NormalizeEmail(string? value) => Clean(value);

    /// <summary>
    /// Digits only (no <c>+</c>, spaces, dashes or brackets), and an Indian number has exactly one stored form
    /// (owner decision, Plan 0013): a leading <c>91</c> on 12 digits or a leading <c>0</c> on 11 digits is dropped, so
    /// <c>+91 98765 00001</c>, <c>098765 00001</c> and <c>98765 00001</c> are the same number and cannot register twice.
    /// </summary>
    public static string? NormalizeMobile(string? value)
    {
        var digits = Digits().Replace(Clean(value) ?? string.Empty, string.Empty);
        digits = digits.Length switch
        {
            12 when digits.StartsWith("91", StringComparison.Ordinal) => digits[2..],
            11 when digits.StartsWith('0') => digits[1..],
            _ => digits
        };

        return digits.Length == 0 ? null : digits;
    }

    public static bool IsValidUserName([NotNullWhen(true)] string? normalized) =>
        normalized is { Length: >= UsernameMinLength and <= UsernameMaxLength }
        && UserNamePattern().IsMatch(normalized);

    /// <summary>Deliberately loose: one <c>@</c>, no whitespace, something on both sides. The framework validates the rest.</summary>
    public static bool IsValidEmail([NotNullWhen(true)] string? normalized) =>
        normalized is { Length: <= EmailMaxLength } && EmailPattern().IsMatch(normalized);

    public static bool IsValidMobile([NotNullWhen(true)] string? normalized) =>
        normalized is { Length: >= MobileMinLength and <= MobileMaxLength };

    public static bool IsValidFullName([NotNullWhen(true)] string? value) =>
        value is { Length: >= 1 and <= FullNameMaxLength };

    private static string? Clean(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed.ToLowerInvariant();
    }

    [GeneratedRegex("^[a-z0-9][a-z0-9._-]*$")]
    private static partial Regex UserNamePattern();

    [GeneratedRegex("^[^\\s@]+@[^\\s@.]+(\\.[^\\s@.]+)+$")]
    private static partial Regex EmailPattern();

    [GeneratedRegex("\\D")]
    private static partial Regex Digits();
}
