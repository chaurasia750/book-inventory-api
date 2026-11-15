using BookInventory.Domain.Core;

namespace BookInventory.Domain.School;

/// <summary>
/// R6: one class of one school (Nursery, 1st, ...), with its place in the school's ordered list. A company-owned child (platform P18) of
/// <see cref="School"/>: it is created, renamed, reordered and removed only through the root, which keeps the names unique
/// inside the school.
/// </summary>
public class SchoolClass : CompanyOwnedChild
{
    public const int NameMaxLength = 50;

    private SchoolClass()
    {
        Name = string.Empty;
        NameNormalized = string.Empty;
    }

    public Guid SchoolId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The name in the form the unique index holds (lower case).</summary>
    public string NameNormalized { get; private set; }

    public int SortOrder { get; private set; }

    /// <summary>R6, R16: the form a class name is compared in (trimmed, single spaces, lower case), or null for an empty text.</summary>
    public static string? KeyOf(string? name) => SchoolRules.ClassKeyOf(name);

    internal static SchoolClass Of(Guid schoolId, string name, int sortOrder) =>
        new() { SchoolId = schoolId, Name = name, NameNormalized = SchoolRules.Normalize(name), SortOrder = sortOrder };

    /// <summary>Keeps the row (and its id) but takes the spelling and the place the Owner gave it this time.</summary>
    internal void Place(string name, int sortOrder)
    {
        Name = name;
        SortOrder = sortOrder;
    }
}
