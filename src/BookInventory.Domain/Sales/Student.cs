using System.Text.RegularExpressions;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Sales;

/// <summary>
/// A pupil of a school who buys at the counter (spec sales R40, R41). A company-owned aggregate root. Two students of a school are
/// the same student when their name (ignoring case and extra spaces) and class are the same; the section never decides (Q26), so a
/// section given later is stored here as the latest value. Created at the counter, never deleted by this module.
/// </summary>
public partial class Student : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int ClassMaxLength = 50;
    public const int SectionMaxLength = 20;
    public const int RollNumberMaxLength = 30;

    private Student()
    {
        Name = string.Empty;
        NameNormalized = string.Empty;
        ClassName = string.Empty;
    }

    public Guid SchoolId { get; private set; }

    public string Name { get; private set; }

    /// <summary>The name in the form two names are compared in: single spaces, lower case.</summary>
    public string NameNormalized { get; private set; }

    public string ClassName { get; private set; }

    public string? Section { get; private set; }

    public string? RollNumber { get; private set; }

    public string? ParentMobile { get; private set; }

    /// <summary>R40: the form a name is compared in; the store finds the student with it.</summary>
    public static string NormalizeName(string? name) => SpaceRun().Replace(name?.Trim() ?? string.Empty, " ").ToLowerInvariant();

    public static Student Create(Guid schoolId, StudentDetails details)
    {
        var student = new Student { SchoolId = schoolId };
        student.Apply(details, firstTime: true);
        return student;
    }

    /// <summary>R40, R41: a later bill brings what it knows; only the values it gives replace the stored ones (the name and class are the identity and never change).</summary>
    public void Learn(StudentDetails details) => Apply(details, firstTime: false);

    private void Apply(StudentDetails details, bool firstTime)
    {
        if (firstTime)
        {
            var name = SpaceRun().Replace(details.Name?.Trim() ?? string.Empty, " ");
            if (name.Length is < NameMinLength or > NameMaxLength)
                throw new ValidationFailureException(CounterMessages.InvalidStudentName);

            var className = details.ClassName?.Trim() ?? string.Empty;
            if (className.Length is 0 or > ClassMaxLength)
                throw new ValidationFailureException(CounterMessages.InvalidStudentClass);

            Name = name;
            NameNormalized = NormalizeName(name);
            ClassName = className;
        }

        Section = Given(details.Section, SectionMaxLength) ?? Section;
        RollNumber = Given(details.RollNumber, RollNumberMaxLength) ?? RollNumber;
        ParentMobile = ContactRules.Mobile(details.ParentMobile, CounterMessages.InvalidStudentMobile) ?? ParentMobile;
    }

    private static string? Given(string? value, int maxLength)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;
        return trimmed.Length <= maxLength ? trimmed : throw new ValidationFailureException(CounterMessages.InvalidStudentField);
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex SpaceRun();
}
