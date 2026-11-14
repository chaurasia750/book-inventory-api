using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.School;

/// <summary>
/// One customer school of one company (spec school R1-R15). A company-owned aggregate root (platform P18): its company is
/// stamped and filtered by the platform. It owns the master data (R2-R5, R7), the ordered class list (R6) and the
/// <c>ACTIVE</c>/<c>INACTIVE</c> status (R8); every rule of those is enforced here, and nothing outside changes its properties.
/// The code is generated once at creation from the company code series (platform P23, prefix <see cref="CodePrefix"/>) and never changes (R15).
/// </summary>
public class School : CompanyOwnedRoot
{
    public const int NameMinLength = 2;
    public const int NameMaxLength = 150;
    public const int CodeMaxLength = 20;

    /// <summary>R15: the prefix of the school codes in the company code series (<c>SCH-0001</c>).</summary>
    public const string CodePrefix = "SCH";

    /// <summary>R6/Q2: the class list every new school starts with.</summary>
    public static readonly IReadOnlyList<string> DefaultClasses =
        ["Nursery", "LKG", "UKG", "1st", "2nd", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th", "11th", "12th"];

    private readonly List<SchoolClass> _classes = [];

    private School()
    {
        Code = string.Empty;
        Name = string.Empty;
        State = IndianState.Default;
    }

    public string Code { get; private set; }

    public string Name { get; private set; }

    public string? Address { get; private set; }

    public string? ContactPerson { get; private set; }

    public string? Mobile { get; private set; }

    public string? Email { get; private set; }

    public string? Gstin { get; private set; }

    public string State { get; private set; }

    public SessionType SessionType { get; private set; }

    public SaleType SaleType { get; private set; }

    public bool IsGstDefault { get; private set; }

    public decimal OpeningBalanceAmount { get; private set; }

    public SchoolStatus Status { get; private set; }

    /// <summary>The live class list in order; soft-deleted rows stay in the backing field until the save and are not shown.</summary>
    public IReadOnlyList<SchoolClass> Classes => _classes.Where(c => !c.IsDeleted).OrderBy(c => c.SortOrder).ToList();

    public bool IsActive => Status == SchoolStatus.Active;

    /// <summary>R2-R8, R15: a new school, active, with the generated code and the default class list.</summary>
    public static School Create(string code, SchoolDetails details)
    {
        var school = new School { Code = code, Status = SchoolStatus.Active };
        school.Apply(details);
        school.ReplaceClasses(DefaultClasses);
        school.AddDomainEvent(new SchoolCreated(school.Id, school.Code, school.Name));
        return school;
    }

    /// <summary>R2-R5, R7: changes the master data; the code, the status and the classes never change here.</summary>
    public void Edit(SchoolDetails details) => Apply(details);

    /// <summary>
    /// R6: the whole class list in the given order; names unique inside the school ignoring case. A class that keeps its
    /// name keeps its row (and its id, so later modules keep pointing at it); a new name is added; a name that is gone is
    /// soft-deleted (platform P4), which is how the row leaves the list without a hard delete.
    /// </summary>
    public void ReplaceClasses(IEnumerable<string> names)
    {
        var cleaned = names.Select(SchoolRules.RequireClassName).ToList();
        var keys = cleaned.Select(SchoolRules.Normalize).ToList();
        if (keys.Distinct().Count() != keys.Count)
            throw new ValidationFailureException(SchoolMessages.DuplicateClass);

        foreach (var existing in _classes.Where(c => !c.IsDeleted && !keys.Contains(c.NameNormalized)))
            existing.MarkDeleted();

        for (var i = 0; i < cleaned.Count; i++)
        {
            var current = _classes.FirstOrDefault(c => !c.IsDeleted && c.NameNormalized == keys[i]);
            if (current is null)
                _classes.Add(SchoolClass.Of(Id, cleaned[i], i + 1));
            else
                current.Place(cleaned[i], i + 1);
        }
    }

    /// <summary>R8: switches the school off; its data and history stay.</summary>
    public void Deactivate()
    {
        if (Status == SchoolStatus.Inactive)
            throw new ConflictException(SchoolMessages.AlreadyInactive);

        Status = SchoolStatus.Inactive;
        AddDomainEvent(new SchoolDeactivated(Id));
    }

    /// <summary>R8: switches the school on again.</summary>
    public void Reactivate()
    {
        if (Status == SchoolStatus.Active)
            throw new ConflictException(SchoolMessages.AlreadyActive);

        Status = SchoolStatus.Active;
        AddDomainEvent(new SchoolReactivated(Id));
    }

    private void Apply(SchoolDetails details)
    {
        Name = SchoolRules.RequireName(details.Name);
        Address = SchoolRules.Optional(details.Address, 500);
        ContactPerson = SchoolRules.Optional(details.ContactPerson, 200);
        Mobile = SchoolRules.Mobile(details.Mobile);
        Email = SchoolRules.Email(details.Email);
        Gstin = SchoolRules.Optional(details.Gstin, 15)?.ToUpperInvariant();
        State = IndianState.Require(details.State);
        SessionType = details.SessionType;
        SaleType = details.SaleType;
        IsGstDefault = details.IsGstDefault;
        OpeningBalanceAmount = SchoolRules.OpeningBalance(details.OpeningBalanceAmount);
    }
}
