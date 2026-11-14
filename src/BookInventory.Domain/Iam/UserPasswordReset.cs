using BookInventory.BuildingBlocks.Domain;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Iam;

/// <summary>
/// R67: the record of one password reset by a person (a Super Admin or an Owner): which account, who reset it and when.
/// Its own root and insert-only, like <see cref="UserStatusHistory"/>; it never holds a password or a hash.
/// </summary>
[AuditIgnore]
public class UserPasswordReset : AggregateRoot
{
    private UserPasswordReset()
    {
    }

    public Guid UserId { get; private set; }

    public Guid ResetBy { get; private set; }

    public DateTime ResetAt { get; private set; }

    public static UserPasswordReset Record(Guid userId, Guid actorId, DateTime utcNow) =>
        new() { UserId = userId, ResetBy = actorId, ResetAt = utcNow };
}
