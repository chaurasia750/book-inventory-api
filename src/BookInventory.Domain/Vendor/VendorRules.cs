using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Iam;
using BookInventory.Domain.Core;

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

    public static string? Mobile(string? value) => ContactRules.Mobile(value, VendorMessages.InvalidMobile);

    public static string? Email(string? value) => ContactRules.Email(value, VendorMessages.InvalidEmail);

    /// <summary>R3: 15 characters in the GST shape, stored in upper case; empty is none.</summary>
    public static string? Gstin(string? value) => ContactRules.Gstin(value, VendorMessages.InvalidGstin);

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
    public static BankAccount? Bank(BankAccount? bank) => BankAccount.Validated(bank, VendorMessages.InvalidBank);

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();



}
