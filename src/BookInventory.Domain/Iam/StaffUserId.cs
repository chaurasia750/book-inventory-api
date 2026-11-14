using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// R65: the login of a company's Staff account: the Owner's user id, a dot, and a short name the Owner chooses
/// ("childrenbookcenter" + "ramesh" gives <c>childrenbookcenter.ramesh</c>). The short name is 2-20 lower-case letters
/// or digits; whether the id is free is <see cref="AppUser"/>'s and the uniqueness check's job (R1).
/// </summary>
public static partial class StaffUserId
{
    public const int ShortNameMinLength = 2;
    public const int ShortNameMaxLength = 20;

    public static string From(string ownerUserName, string? shortName)
    {
        var cleaned = shortName?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!ShortNamePattern().IsMatch(cleaned))
            throw new ValidationFailureException(IamMessages.StaffShortNameInvalid);

        return $"{ownerUserName}.{cleaned}";
    }

    [GeneratedRegex("^[a-z0-9]{2,20}$")]
    private static partial Regex ShortNamePattern();
}
