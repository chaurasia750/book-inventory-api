using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Core;

/// <summary>
/// One recorded change of one audited row (spec core A1-A6): which entity, what happened, the old and new value as JSON
/// text (changed fields only for an update, A2), who did it (a snapshot of the id and user name, A4, AD3), the company of
/// the row (A5) and when (<c>created_at</c>). It is insert-only: nothing edits or deletes it (A6), and it is not audited
/// itself.
/// </summary>
[AuditIgnore]
public class AuditEntry : AggregateRoot
{
    public const int EntityTypeMaxLength = 100;
    public const int ActorNameMaxLength = 200;

    private AuditEntry() => EntityType = string.Empty;

    public string EntityType { get; private set; }

    public Guid EntityId { get; private set; }

    public AuditAction Action { get; private set; }

    public string? OldValue { get; private set; }

    public string? NewValue { get; private set; }

    /// <summary>Null for a background job and for the hidden system account (A4).</summary>
    public Guid? ActorId { get; private set; }

    public string? ActorName { get; private set; }

    public bool IsSystem { get; private set; }

    /// <summary>A5: the company of the changed row, a plain id; null for platform rows.</summary>
    public Guid? CompanyId { get; private set; }

    public static AuditEntry Record(AuditChange change, AuditActorInfo actor, Guid? companyId) =>
        new()
        {
            EntityType = change.EntityType,
            EntityId = change.EntityId,
            Action = change.Action,
            OldValue = change.OldValue,
            NewValue = change.NewValue,
            ActorId = actor.IsSystem ? null : actor.UserId,
            ActorName = actor.IsSystem ? null : Truncate(actor.UserName),
            IsSystem = actor.IsSystem,
            CompanyId = companyId
        };

    private static string? Truncate(string? name) =>
        name is { Length: > ActorNameMaxLength } ? name[..ActorNameMaxLength] : name;
}

/// <summary>What changed: the entity, the action and the JSON text of the old and new value (A2).</summary>
public sealed record AuditChange(string EntityType, Guid EntityId, AuditAction Action, string? OldValue, string? NewValue);

/// <summary>Who made the change (A4): a user, or "System" (a job, or the hidden system account).</summary>
public sealed record AuditActorInfo(Guid? UserId, string? UserName, bool IsSystem);
