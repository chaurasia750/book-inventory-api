namespace BookInventory.Domain.Core;

/// <summary>
/// Spec platform P23: the counter a company's generated codes of one prefix come from (<c>SCH-0001</c>, <c>VEN-0001</c>, ...),
/// one row per company and prefix. The port that hands out the numbers reads and increases it under a lock inside the
/// creating transaction, so two rows created at the same moment get different numbers; a refused creation rolls the
/// increase back. A number is never reused. It is a counter, not business data, so it is not audited.
/// </summary>
[AuditIgnore]
public class CompanyCodeSeries : CompanyOwnedRoot
{
    public const int MinDigits = 4;
    public const int PrefixMaxLength = 10;

    private CompanyCodeSeries()
    {
        Prefix = string.Empty;
    }

    public string Prefix { get; private set; }

    public int LastNumber { get; private set; }

    public static CompanyCodeSeries Start(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix) || prefix.Length > PrefixMaxLength)
            throw new ArgumentException("A code prefix is 1-10 characters.", nameof(prefix));

        return new CompanyCodeSeries { Prefix = prefix.Trim().ToUpperInvariant() };
    }

    /// <summary>The next code: the prefix, a hyphen and the number with at least four digits (more when needed).</summary>
    public string Next()
    {
        LastNumber++;
        return $"{Prefix}-{LastNumber.ToString(new string('0', MinDigits))}";
    }
}
