using System.Text.Json;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// Spec core N1, N2, N8: records notifications in the caller's transaction and lets an operator cancel or requeue one.
/// Nothing is sent here, and nothing here saves - the request transaction commits the row together with the business
/// change that asked for it (<c>TransactionFilter</c>), so a rolled-back change leaves no notification behind.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(INotificationService))]
public sealed class NotificationService(
    INotificationStore store,
    NotificationSettings settings,
    TimeProvider timeProvider) : INotificationService
{
    public async Task<QueuedNotification> QueueAsync(QueueNotification request, CancellationToken cancellationToken = default)
    {
        var existing = await store.FindByIdempotencyKeyAsync(request.IdempotencyKey.Trim(), cancellationToken);
        if (existing is not null)
            return new QueuedNotification(existing.Id, existing.Status);

        var template = await store.FindTemplateAsync(request.TemplateCode.Trim().ToUpperInvariant(), cancellationToken);
        if (template is not { IsActive: true })
            throw new NotFoundException(CoreMessages.TemplateNotFound);

        var notification = Notification.Queue(
            new NewNotification(
                template.Code,
                request.Recipient,
                JsonSerializer.Serialize(request.Variables),
                request.IdempotencyKey,
                request.CorrelationId),
            new NotificationPolicy(template.IsSensitive, settings.DefaultMaxRetry),
            timeProvider.GetUtcNow().UtcDateTime);

        await store.AddAsync(notification, cancellationToken);
        return new QueuedNotification(notification.Id, notification.Status);
    }

    public async Task CancelAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        (await Require(notificationId, cancellationToken)).Cancel();

    public async Task RequeueAsync(Guid notificationId, CancellationToken cancellationToken = default) =>
        (await Require(notificationId, cancellationToken)).Requeue(timeProvider.GetUtcNow().UtcDateTime);

    private async Task<Notification> Require(Guid notificationId, CancellationToken cancellationToken) =>
        await store.FindByIdAsync(notificationId, cancellationToken)
        ?? throw new NotFoundException(CoreMessages.NotFound);
}
