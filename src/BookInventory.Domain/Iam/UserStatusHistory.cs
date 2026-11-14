using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// The immutable record of one status change of one user (R33): who changed it, from where to where, why and when.
/// <c>app_user.status_*</c> only holds the current state; this is the history shown on the user's detail page, and it
/// is never edited or deleted.
/// <para>
/// Its own root rather than a child of <see cref="AppUser"/>: the change is written by a different use case than the
/// one that changes the status (Plan 0006), and a nullable <c>changed_by</c> (SYSTEM) has no parent to hang from.
/// </para>
/// </summary>
public class UserStatusHistory : AggregateRoot
{
    private UserStatusHistory() => Reason = string.Empty;

    public Guid UserId { get; private set; }

    public AccountStatus FromStatus { get; private set; }

    public AccountStatus ToStatus { get; private set; }

    /// <summary>Reject / deactivate / suspend reason, or the approval or activation note. Empty only if none was given.</summary>
    public string Reason { get; private set; }

    /// <summary>End of a suspension, carried over so the history shows it without joining the user row.</summary>
    public DateTime? Until { get; private set; }

    /// <summary>Who made the change. Null is <c>SYSTEM</c>: the automatic lift of an expired suspension (R32).</summary>
    public Guid? ChangedBy { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public static UserStatusHistory Record(Guid userId, StatusTransition transition, Guid actorId, DateTime utcNow)
    {
        if (transition.From == transition.To)
            throw new ValidationFailureException(IamMessages.UserInvalidTransition);

        return new UserStatusHistory
        {
            UserId = userId,
            FromStatus = transition.From,
            ToStatus = transition.To,
            Reason = transition.Reason?.Trim() ?? string.Empty,
            Until = transition.Until,
            ChangedBy = actorId == Guid.Empty ? null : actorId,
            ChangedAt = utcNow
        };
    }
}
