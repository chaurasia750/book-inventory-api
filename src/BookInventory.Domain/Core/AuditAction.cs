namespace BookInventory.Domain.Core;

/// <summary>What happened to an audited row (spec core A1): stored as UPPER_SNAKE text.</summary>
public enum AuditAction
{
    Created,

    Updated,

    Deleted
}
