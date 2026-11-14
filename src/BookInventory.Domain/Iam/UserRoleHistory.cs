using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Iam;

/// <summary>
/// The immutable record of one role change (R6). Written whenever a Super Admin moves a user between the staff roles,
/// never for a <c>MEMBER</c>, and never edited or deleted. Its own root, for the same reason as
/// <see cref="UserStatusHistory"/>.
/// </summary>
[AuditIgnore]
public class UserRoleHistory : AggregateRoot
{
    private UserRoleHistory() => Reason = string.Empty;

    public Guid UserId { get; private set; }

    public Role FromRole { get; private set; }

    public Role ToRole { get; private set; }

    public string Reason { get; private set; }

    public Guid? ChangedBy { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public static UserRoleHistory Record(Guid userId, RoleTransition transition, Guid actorId, DateTime utcNow)
    {
        if (transition.From == transition.To)
            throw new ValidationFailureException(IamMessages.UserRoleUnchanged);
        if (transition.From == Role.Member || transition.To == Role.Member)
            throw new ValidationFailureException(IamMessages.UserMemberRoleImmutable);

        return new UserRoleHistory
        {
            UserId = userId,
            FromRole = transition.From,
            ToRole = transition.To,
            Reason = transition.Reason?.Trim() ?? string.Empty,
            ChangedBy = actorId == Guid.Empty ? null : actorId,
            ChangedAt = utcNow
        };
    }
}
