using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;

namespace BookInventory.Domain.Core;

/// <summary>
/// The value rules of the contact and tax identifiers every party keeps (a vendor's, a school's, a company's), written once: a mobile
/// number and an email address as <c>iam</c> stores them, a GSTIN in the GST shape and a PAN. Each method returns the stored form
/// (<c>null</c> for an empty value) or throws the module's own message code (a <c>400</c>), so a module owns its texts, not the rule.
/// </summary>
public static partial class ContactRules
{
    /// <summary>10 to 15 digits in the one stored form of a mobile number (<see cref="IdentityNormalizer"/>).</summary>
    public static string? Mobile(string? value, string invalidCode)
    {
        var normalized = IdentityNormalizer.NormalizeMobile(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidMobile(normalized) ? normalized : throw new ValidationFailureException(invalidCode);
    }

    /// <summary>A valid email address in its stored form (<see cref="IdentityNormalizer"/>).</summary>
    public static string? Email(string? value, string invalidCode)
    {
        var normalized = IdentityNormalizer.NormalizeEmail(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidEmail(normalized) ? normalized : throw new ValidationFailureException(invalidCode);
    }

    /// <summary>15 characters in the GST shape, stored in upper case.</summary>
    public static string? Gstin(string? value, string invalidCode) => Shaped(value, GstinShape(), invalidCode);

    /// <summary>Ten characters: five letters, four digits and a letter, stored in upper case.</summary>
    public static string? Pan(string? value, string invalidCode) => Shaped(value, PanShape(), invalidCode);

    private static string? Shaped(string? value, Regex shape, string invalidCode)
    {
        var trimmed = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return shape.IsMatch(trimmed) ? trimmed : throw new ValidationFailureException(invalidCode);
    }

    [GeneratedRegex(@"^\d{2}[A-Z]{5}\d{4}[A-Z][1-9A-Z]Z[0-9A-Z]$")]
    private static partial Regex GstinShape();

    [GeneratedRegex(@"^[A-Z]{5}\d{4}[A-Z]$")]
    private static partial Regex PanShape();
}
