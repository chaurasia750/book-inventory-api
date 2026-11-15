using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Planning;

/// <summary>
/// Spec planning R2-R5: the books one class of one school uses in one session, each with a quantity per student, and the
/// approximate number of students. It is a help for orders (sales R22), which copy its lines; changing it never changes an order.
/// </summary>
public class ClassBookList : CompanyOwnedRoot
{
    public const int MaxLines = 200;
    public const int MinQuantityPerStudent = 1;
    public const int MaxQuantityPerStudent = 50;
    public const int MaxStudentEstimate = 5000;
    public const int SessionMaxLength = 20;
    public const int ClassNameMaxLength = 50;

    private readonly List<ClassBookListLine> _lines = [];

    private ClassBookList()
    {
        Session = string.Empty;
        ClassName = string.Empty;
    }

    public Guid SchoolId { get; private set; }

    public string Session { get; private set; }

    public string ClassName { get; private set; }

    public int? StudentEstimate { get; private set; }

    /// <summary>The live lines in their order (replaced lines stay soft-deleted in the backing field).</summary>
    public IReadOnlyList<ClassBookListLine> Lines => _lines.Where(l => !l.IsDeleted).OrderBy(l => l.SortOrder).ToList();

    /// <summary>R2, R3: a new list of a school's class in a session (the use case has checked the school, session and class).</summary>
    public static ClassBookList Start(Guid schoolId, string session, string className)
    {
        if (string.IsNullOrWhiteSpace(session) || session.Length > SessionMaxLength)
            throw new ValidationFailureException(PlanningMessages.InvalidSession);
        if (string.IsNullOrWhiteSpace(className) || className.Length > ClassNameMaxLength)
            throw new ValidationFailureException(PlanningMessages.InvalidClass);

        return new ClassBookList { SchoolId = schoolId, Session = session.Trim(), ClassName = className.Trim() };
    }

    /// <summary>R2, R3, R5: replaces the books and the estimate; the same item twice becomes one line with the quantities added.</summary>
    public void Replace(int? studentEstimate, IReadOnlyList<BookListLineInput> lines)
    {
        if (studentEstimate is < 0 or > MaxStudentEstimate)
            throw new ValidationFailureException(PlanningMessages.InvalidEstimate);

        ReplaceLines(lines);
        StudentEstimate = studentEstimate;
    }

    /// <summary>R4: takes the books of another list (a copy), keeping this list's own estimate.</summary>
    public void CopyBooksFrom(ClassBookList source) =>
        ReplaceLines(source.Lines.Select(l => new BookListLineInput(l.ItemId, l.ItemCode, l.ItemName, l.QuantityPerStudent)).ToList());

    private void ReplaceLines(IReadOnlyList<BookListLineInput> lines)
    {
        var merged = lines
            .GroupBy(l => l.ItemId)
            .Select(g => g.First() with { QuantityPerStudent = g.Sum(l => l.QuantityPerStudent) })
            .ToList();
        if (merged.Count > MaxLines)
            throw new ValidationFailureException(PlanningMessages.TooManyLines);
        if (lines.Any(l => l.QuantityPerStudent < MinQuantityPerStudent) || merged.Any(l => l.QuantityPerStudent > MaxQuantityPerStudent))
            throw new ValidationFailureException(PlanningMessages.InvalidQuantity);

        foreach (var old in _lines.Where(l => !l.IsDeleted))
            old.MarkDeleted();
        _lines.AddRange(merged.Select((l, i) => ClassBookListLine.Of(Id, i + 1, l)));
    }
}
