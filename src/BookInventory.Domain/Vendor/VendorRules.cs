using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;

namespace BookInventory.Domain.Vendor;

/// <summary>
/// The validation and normalisation of vendor values (R2-R6), written once. Mobile and email use the same rules as <c>iam</c>
/// (<see cref="IdentityNormalizer"/>), so one number has one stored form across the system.
/// </summary>
internal static partial class VendorRules
{
    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < Vendor.NameMinLength or > Vendor.NameMaxLength)
            throw new ValidationFailureException(VendorMessages.InvalidName);
        return cleaned;
    }

    /// <summary>R4: the form two names are compared in: single spaces, lower case.</summary>
    public static string NormalizeName(string? name) => SpaceRun().Replace(name?.Trim() ?? string.Empty, " ").ToLowerInvariant();

    public static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(VendorMessages.InvalidField);
    }

    public static string? Mobile(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeMobile(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidMobile(normalized) ? normalized : throw new ValidationFailureException(VendorMessages.InvalidMobile);
    }

    public static string? Email(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeEmail(value);
        if (normalized is null)
            return null;
        return IdentityNormalizer.IsValidEmail(normalized) ? normalized : throw new ValidationFailureException(VendorMessages.InvalidEmail);
    }

    /// <summary>R3: 15 characters in the GST shape, stored in upper case; empty is none.</summary>
    public static string? Gstin(string? value)
    {
        var trimmed = value?.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return GstinShape().IsMatch(trimmed) ? trimmed : throw new ValidationFailureException(VendorMessages.InvalidGstin);
    }

    /// <summary>R6: zero or more, two decimals (an amount with more decimals is refused, never rounded silently).</summary>
    public static decimal OpeningBalance(decimal amount)
    {
        if (amount < 0m || decimal.Round(amount, 2) != amount)
            throw new ValidationFailureException(VendorMessages.InvalidOpeningBalance);
        return amount;
    }

    /// <summary>
    /// R5: the five bank values all present or all absent. Holder, bank and branch are trimmed text (at most 150 characters),
    /// the account number is 9-18 digits and the IFSC is 4 letters, a zero and 6 letters or digits (upper case).
    /// </summary>
    public static VendorBank? Bank(VendorBank? bank)
    {
        if (bank is null)
            return null;

        var holder = Optional(bank.AccountHolder, Vendor.BankTextMaxLength);
        var name = Optional(bank.BankName, Vendor.BankTextMaxLength);
        var branch = Optional(bank.Branch, Vendor.BankTextMaxLength);
        var number = bank.AccountNumber?.Trim();
        var ifsc = bank.Ifsc?.Trim().ToUpperInvariant();
        var given = new[] { holder, name, branch, number, ifsc }.Count(v => !string.IsNullOrEmpty(v));
        if (given == 0)
            return null;

        var valid = given == 5 && AccountNumberShape().IsMatch(number!) && IfscShape().IsMatch(ifsc!);
        return valid ? new VendorBank(holder, name, branch, number, ifsc) : throw new ValidationFailureException(VendorMessages.InvalidBank);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();

    [GeneratedRegex(@"^\d{2}[A-Z]{5}\d{4}[A-Z][1-9A-Z]Z[0-9A-Z]$")]
    private static partial Regex GstinShape();

    [GeneratedRegex(@"^\d{9,18}$")]
    private static partial Regex AccountNumberShape();

    [GeneratedRegex(@"^[A-Z]{4}0[A-Z0-9]{6}$")]
    private static partial Regex IfscShape();
}
