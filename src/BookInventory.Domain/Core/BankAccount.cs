using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Core;

/// <summary>
/// A bank account, written once for every module that keeps one (a vendor's, vendor R5; a company's, company R19): account holder,
/// bank, branch, account number (9-18 digits) and IFSC (four letters, a zero, six letters or digits, upper case). The five values
/// are all present or all absent. The account number is sensitive: lists show <see cref="Mask"/>, never the number.
/// </summary>
public sealed partial record BankAccount(string? AccountHolder, string? BankName, string? Branch, string? AccountNumber, string? Ifsc)
{
    /// <summary>The longest holder, bank or branch text.</summary>
    public const int TextMaxLength = 150;

    /// <summary>Every place that lists an account shows this form (<c>XXXXXX1234</c>), never the number.</summary>
    public static string? Mask(string? accountNumber) =>
        string.IsNullOrEmpty(accountNumber)
            ? null
            : new string('X', Math.Max(accountNumber.Length - 4, 0)) + accountNumber[^Math.Min(4, accountNumber.Length)..];

    /// <summary>
    /// The account as the owning module stores it: <c>null</c> when none of the five values is given, the trimmed values when all
    /// five are given and well formed, and <paramref name="invalidCode"/> (a <c>400</c>) for anything in between.
    /// </summary>
    public static BankAccount? Validated(BankAccount? raw, string invalidCode)
    {
        if (raw is null)
            return null;

        var holder = Text(raw.AccountHolder);
        var bank = Text(raw.BankName);
        var branch = Text(raw.Branch);
        var number = raw.AccountNumber?.Trim();
        var ifsc = raw.Ifsc?.Trim().ToUpperInvariant();
        var given = new[] { holder, bank, branch, number, ifsc }.Count(v => !string.IsNullOrEmpty(v));
        if (given == 0)
            return null;

        var valid = given == 5 && holder!.Length <= TextMaxLength && bank!.Length <= TextMaxLength && branch!.Length <= TextMaxLength
                    && AccountNumberShape().IsMatch(number!) && IfscShape().IsMatch(ifsc!);
        return valid ? new BankAccount(holder, bank, branch, number, ifsc) : throw new ValidationFailureException(invalidCode);
    }

    private static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"^\d{9,18}$")]
    private static partial Regex AccountNumberShape();

    [GeneratedRegex(@"^[A-Z]{4}0[A-Z0-9]{6}$")]
    private static partial Regex IfscShape();
}
