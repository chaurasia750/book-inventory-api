using BookInventory.Domain.School;

namespace BookInventory.Application.School;

/// <summary>
/// R18: the capability other modules (the sales module first) use to know a school's session on a date, with the sessions before
/// and after it; they never compute it themselves. Only the session's company is seen; an unknown school gives <c>null</c>.
/// </summary>
public interface ISchoolSessions
{
    Task<SessionChoice?> SessionOfAsync(Guid schoolId, DateOnly date, CancellationToken cancellationToken = default);
}
