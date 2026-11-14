using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>A8: the list filters and paging. <see cref="From"/> and <see cref="To"/> are instants (UTC), both inclusive.</summary>
public sealed record AuditSearchQuery
{
    public string? EntityType { get; init; }

    public Guid? EntityId { get; init; }

    /// <summary>The actor of the change.</summary>
    public Guid? UserId { get; init; }

    /// <summary>Only a Super Admin's value counts; an Owner's is replaced by their own company (A7).</summary>
    public Guid? CompanyId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>A4: the actor of an entry; a job and the hidden system account arrive with <see cref="IsSystem"/> and no id.</summary>
public sealed record AuditActorResponse(Guid? UserId, string? Name, bool IsSystem);

/// <summary>A1/A8: one row of the audit list. Old and new value are JSON text, as stored.</summary>
public sealed record AuditEntryResponse(
    Guid Id,
    DateTime CreatedAt,
    AuditActorResponse Actor,
    string EntityType,
    Guid EntityId,
    AuditAction Action,
    string? OldValue,
    string? NewValue,
    Guid? CompanyId);
