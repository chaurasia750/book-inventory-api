using BookInventory.Application.Price;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Planning;
using BookInventory.Domain.School;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Planning;

/// <summary>
/// Spec planning R1, R2: the one place that checks what a list request names, through the school capabilities (school R16, R18):
/// the school of the session's company (else 404), a session written the way that school counts them, and one of its classes.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class BookListScope(ISchoolDirectory schools, ISchoolSessions sessions, BusinessCalendar calendar)
{
    /// <summary>The school's current session label (R7: the default of a read).</summary>
    public async Task<string> CurrentSessionAsync(Guid schoolId, CancellationToken cancellationToken)
    {
        var choice = await sessions.SessionOfAsync(schoolId, calendar.Today(), cancellationToken)
                     ?? throw new NotFoundException(PlanningMessages.SchoolNotFound);
        return choice.Session;
    }

    /// <summary>R2: the session as written, when it is the school's kind of session; 404 for an unknown school.</summary>
    public async Task<string> SessionAsync(Guid schoolId, string? session, CancellationToken cancellationToken)
    {
        var current = await CurrentSessionAsync(schoolId, cancellationToken);
        var text = session?.Trim();
        if (SchoolSession.TypeOf(text) is not { } type || type != SchoolSession.TypeOf(current))
            throw new ValidationFailureException(PlanningMessages.InvalidSession);

        return text!;
    }

    /// <summary>R2: the school's own spelling of the class, or 400.</summary>
    public async Task<string> ClassAsync(Guid schoolId, string? className, CancellationToken cancellationToken) =>
        await schools.ClassOfAsync(schoolId, className, cancellationToken)
        ?? throw new ValidationFailureException(PlanningMessages.InvalidClass);

    /// <summary>R4, R7: the school's classes in its order.</summary>
    public Task<IReadOnlyList<string>> ClassesAsync(Guid schoolId, CancellationToken cancellationToken) =>
        schools.ClassesOfAsync(schoolId, cancellationToken);
}
