using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R38: a new counter. The session is the school's session on the start date unless the Owner chooses the one before or after it.</summary>
public sealed record SaveCounterRequest(
    [param: Required, StringLength(Counter.NameMaxLength)] string Name,
    [param: Required] Guid? SchoolId,
    [param: Required] DateOnly? StartDate,
    string? Session = null,
    DateOnly? EndDate = null);

/// <summary>R49: the counter list filters and paging. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record CounterSearchQuery
{
    /// <summary>Free text over the counter name and the school's name and code.</summary>
    public string? Search { get; init; }

    public Guid? SchoolId { get; init; }

    public string? Session { get; init; }

    public CounterStatus? Status { get; init; }

    /// <summary>Whitelisted sort: <c>CreatedAt</c>, <c>Name</c> or <c>StartDate</c>, then <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "CreatedAt DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>R49: one counter as the list and the detail show it, with the school's name read through the school directory.</summary>
public sealed record CounterResponse(
    Guid Id,
    string Name,
    Guid SchoolId,
    string? SchoolCode,
    string? SchoolName,
    string Session,
    DateOnly StartDate,
    DateOnly? EndDate,
    CounterStatus Status,
    bool HasBills,
    DateTime CreatedAt);

/// <summary>R40, R41: a student as a bill gives it: the name and class are the identity, the rest is optional.</summary>
public sealed record StudentInput(
    [param: Required, StringLength(Student.NameMaxLength)] string Name,
    [param: Required, StringLength(Student.ClassMaxLength)] string ClassName,
    string? Section = null,
    string? RollNumber = null,
    string? ParentMobile = null)
{
    public StudentDetails ToDetails(string className) => new(Name, className, Section, RollNumber, ParentMobile);
}

/// <summary>R41: the suggestion query: at least two letters of a name for one school, optionally one class.</summary>
public sealed record StudentSuggestQuery
{
    [Required]
    public Guid? SchoolId { get; init; }

    public string? Text { get; init; }

    public string? ClassName { get; init; }
}

/// <summary>R41: one suggested student.</summary>
public sealed record StudentSuggestion(Guid Id, string Name, string ClassName, string? Section, string? RollNumber);
