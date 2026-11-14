using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Core;

/// <summary>The data side of the notification use cases and the worker (spec core N1, N2, N5, N13).</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(INotificationStore))]
public sealed class NotificationStore(IUnitOfWork unitOfWork) : INotificationStore
{
    private readonly IRepository<Notification> _notifications = unitOfWork.Repository<Notification>();
    private readonly IRepository<NotificationTemplate> _templates = unitOfWork.Repository<NotificationTemplate>();

    public Task<Notification?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _notifications.IncludeMultiple(n => n.Logs).FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public Task<Notification?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default) =>
        _notifications.TableNoTracking.FirstOrDefaultAsync(n => n.IdempotencyKey == idempotencyKey, cancellationToken);

    public Task AddAsync(Notification notification, CancellationToken cancellationToken = default) =>
        _notifications.AddAsync(notification, cancellationToken);

    public Task<Guid[]> FindDueIdsAsync(DateTime now, DateTime stuckBefore, int take, CancellationToken cancellationToken = default) =>
        _notifications.TableNoTracking
            .Where(Notification.IsDue(now, stuckBefore))
            .OrderBy(n => n.CreatedAt)
            .Select(n => n.Id)
            .Take(take)
            .ToArrayAsync(cancellationToken);

    public Task<NotificationTemplate?> FindTemplateAsync(string code, CancellationToken cancellationToken = default) =>
        _templates.TableNoTracking.FirstOrDefaultAsync(t => t.Code == code, cancellationToken);

    public Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default) =>
        _templates.AddAsync(template, cancellationToken);

    public async Task<HashSet<string>> FindTemplateCodesAsync(CancellationToken cancellationToken = default) =>
        (await _templates.TableNoTracking.Select(t => t.Code).ToArrayAsync(cancellationToken)).ToHashSet();
}
