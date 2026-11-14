namespace BookInventory.Domain.Vendor;

/// <summary>
/// R5: the five values of a vendor's bank account as they arrive. They are all present or all absent; the aggregate
/// validates them (<see cref="VendorRules.Bank"/>). The account number is sensitive: it is masked wherever it is listed.
/// </summary>
public sealed record VendorBank(string? AccountHolder, string? BankName, string? Branch, string? AccountNumber, string? Ifsc)
{
    /// <summary>R5: every place that lists a vendor shows this form (<c>XXXXXX1234</c>), never the number.</summary>
    public static string? Mask(string? accountNumber) =>
        string.IsNullOrEmpty(accountNumber)
            ? null
            : new string('X', Math.Max(accountNumber.Length - 4, 0)) + accountNumber[^Math.Min(4, accountNumber.Length)..];
}
