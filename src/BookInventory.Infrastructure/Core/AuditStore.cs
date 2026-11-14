using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// The read side of the audit trail (spec core A6-A8): projections only, nothing tracked, newest first. It has no method
/// that writes, so no use case can change an entry; the entries are written by <c>AppDbContext</c> when a change is saved.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IAuditStore))]
public sealed class AuditStore(IUnitOfWork unitOfWork) : IAuditStore
{
    private readonly IRepository<AuditEntry> _entries = unitOfWork.Repository<AuditEntry>();

    public async Task<IPagedList<AuditEntryResponse>> SearchAsync(AuditSearchQuery query, CancellationToken cancellationToken = default)
    {
        var entityType = query.EntityType?.Trim();
        var entityId = query.EntityId;
        var userId = query.UserId;
        var companyId = query.CompanyId;
        var from = query.From;
        var to = query.To;

        return await _entries.TableNoTracking
            .Where(e => string.IsNullOrEmpty(entityType) || e.EntityType == entityType)
            .Where(e => entityId == null || e.EntityId == entityId)
            .Where(e => userId == null || e.ActorId == userId)
            .Where(e => companyId == null || e.CompanyId == companyId)
            .Where(e => from == null || e.CreatedAt >= from)
            .Where(e => to == null || e.CreatedAt <= to)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new AuditEntryResponse(
                e.Id, e.CreatedAt, new AuditActorResponse(e.ActorId, e.ActorName, e.IsSystem),
                e.EntityType, e.EntityId, e.Action, e.OldValue, e.NewValue, e.CompanyId))
            .ToPagedListAsync(query.PageIndex < 1 ? 1 : query.PageIndex, query.PageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<string>> EntityTypesAsync(Guid? companyId, CancellationToken cancellationToken = default) =>
        await _entries.TableNoTracking
            .Where(e => companyId == null || e.CompanyId == companyId)
            .Select(e => e.EntityType)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync(cancellationToken);
}
