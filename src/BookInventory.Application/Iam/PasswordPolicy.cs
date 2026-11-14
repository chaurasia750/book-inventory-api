namespace BookInventory.Application.Iam;

/// <summary>
/// R16: at least 8 characters with an upper case letter, a lower case letter and a digit. The values come from
/// <c>Auth:Password:*</c> (R54), never from a constant here, so the policy is one setting and one place.
/// <para>
/// Every rule is reported separately by <see cref="Validate"/> so the message catalogue can name what is missing
/// instead of saying "password is weak" (R55). A password is never echoed back into the result.
/// </para>
/// </summary>
public sealed record PasswordPolicy(
    int MinimumLength,
    int RequiredDigits,
    int RequiredLowerCase,
    int RequiredUpperCase)
{
    /// <summary>Rules of R16, in the order a user should fix them.</summary>
    public IReadOnlyList<string> Validate(string? password)
    {
        var failures = new List<string>(4);
        if (string.IsNullOrEmpty(password) || password.Length < MinimumLength)
            failures.Add(IamPasswordCodes.TooShort);

        if (CountMatching(password, char.IsDigit) < RequiredDigits)
            failures.Add(IamPasswordCodes.MissingDigit);

        if (CountMatching(password, char.IsLower) < RequiredLowerCase)
            failures.Add(IamPasswordCodes.MissingLowerCase);

        if (CountMatching(password, char.IsUpper) < RequiredUpperCase)
            failures.Add(IamPasswordCodes.MissingUpperCase);

        return failures;
    }

    public bool IsValid(string? password) => Validate(password).Count == 0;

    private static int CountMatching(string? value, Func<char, bool> predicate)
    {
        if (string.IsNullOrEmpty(value))
            return 0;

        var count = 0;
        foreach (var c in value)
        {
            if (predicate(c))
                count++;
        }

        return count;
    }
}

/// <summary>Message codes of the password policy, part of the R55 catalogue.</summary>
public static class IamPasswordCodes
{
    public const string TooShort = "iam.password.too_short";
    public const string MissingDigit = "iam.password.missing_digit";
    public const string MissingLowerCase = "iam.password.missing_lowercase";
    public const string MissingUpperCase = "iam.password.missing_uppercase";
    public const string SameAsCurrent = "iam.password.same_as_current";
}
