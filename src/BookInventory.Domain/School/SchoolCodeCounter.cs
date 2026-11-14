using BookInventory.Domain.Core;

namespace BookInventory.Domain.School;

/// <summary>
/// R15/D5: the counter a company's school codes come from, one row per company. The use case reads and increases it under
/// a lock inside the creating transaction, so two schools created at the same moment get different numbers; a refused
/// creation rolls the increase back, so a number is not burnt. A number is never reused.
/// </summary>
public class SchoolCodeCounter : CompanyOwnedRoot
{
    public const string Prefix = "SCH-";
    public const int MinDigits = 4;

    private SchoolCodeCounter()
    {
    }

    public int LastNumber { get; private set; }

    public static SchoolCodeCounter Start() => new();

    /// <summary>The next code of this company: <c>SCH-0001</c>, <c>SCH-0002</c>, ... (at least four digits, more when needed).</summary>
    public string Next()
    {
        LastNumber++;
        return Prefix + LastNumber.ToString(new string('0', MinDigits));
    }
}
