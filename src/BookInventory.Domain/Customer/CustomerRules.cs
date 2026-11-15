using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Customer;

/// <summary>
/// The validation and normalisation of customer values (R2-R7), written once. The mobile uses the same rule as <c>iam</c>,
/// <c>school</c> and <c>vendor</c> (<see cref="ContactRules"/>), so one number has one stored form across the system.
/// </summary>
internal static partial class CustomerRules
{
    public static string RequireName(string? name)
    {
        var cleaned = SpaceRun().Replace(name?.Trim() ?? string.Empty, " ");
        if (cleaned.Length is < Customer.NameMinLength or > Customer.NameMaxLength)
            throw new ValidationFailureException(CustomerMessages.InvalidName);
        return cleaned;
    }

    /// <summary>The form a name is compared and searched in: single spaces, lower case.</summary>
    public static string NormalizeName(string? name) => SpaceRun().Replace(name?.Trim() ?? string.Empty, " ").ToLowerInvariant();

    public static string? Optional(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(CustomerMessages.InvalidField);
    }

    /// <summary>R2, R3: required; 10-15 digits in the one stored form.</summary>
    public static string RequireMobile(string? value) =>
        ContactRules.Mobile(value, CustomerMessages.InvalidMobile) ?? throw new ValidationFailureException(CustomerMessages.MobileRequired);

    /// <summary>R4: a class belongs to a school; a class without a school is refused.</summary>
    public static string? ClassOf(Guid? schoolId, string? className)
    {
        var cleaned = Optional(className, Customer.ClassMaxLength);
        return cleaned is not null && schoolId is null ? throw new ValidationFailureException(CustomerMessages.InvalidClass) : cleaned;
    }

    /// <summary>R6: zero or more, two decimals (an amount with more decimals is refused, never rounded silently).</summary>
    public static decimal OpeningBalance(decimal amount)
    {
        if (amount < 0m || decimal.Round(amount, 2) != amount)
            throw new ValidationFailureException(CustomerMessages.InvalidOpeningBalance);
        return amount;
    }

    /// <summary>R7: none, or more than zero with two decimals.</summary>
    public static decimal? CreditLimit(decimal? amount)
    {
        if (amount is null)
            return null;
        if (amount <= 0m || decimal.Round(amount.Value, 2) != amount)
            throw new ValidationFailureException(CustomerMessages.InvalidCreditLimit);
        return amount;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
