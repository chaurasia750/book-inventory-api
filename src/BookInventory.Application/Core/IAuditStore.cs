using BookInventory.BuildingBlocks.Domain.Models;

namespace BookInventory.Application.Core;

/// <summary>
/// The read side of the audit trail (spec core A6-A8). There is no way to add, change or delete an entry through it: the
/// entries are written by the persistence layer when a change is saved, and are insert-only.
/// </summary>
public interface IAuditStore
{
    /// <summary>A8: the paged entries newest first, filtered as asked. The caller has already applied the company scope (A7).</summary>
    Task<IPagedList<AuditEntryResponse>> SearchAsync(AuditSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>A8: the entity types present in the entries of <paramref name="companyId"/> (all companies when null).</summary>
    Task<IReadOnlyList<string>> EntityTypesAsync(Guid? companyId, CancellationToken cancellationToken = default);
}
