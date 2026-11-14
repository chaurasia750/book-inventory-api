using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// Spec core N5-N7: sends one notification. The worker asks <see cref="FindDueAsync"/> for the ids of what is due and
/// then calls <see cref="ProcessAsync"/> for each in a scope of its own, so one broken notification can neither poison
/// the change tracker of the next nor stop the batch.
/// <para>
/// Two saves per notification, on purpose: the first commits the claim (<c>PROCESSING</c>) before anything is sent, and
/// the optimistic-concurrency token makes a second worker that claimed the same row fail there instead of sending it
/// twice (NAC9); the second commits the outcome and the attempt log.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class NotificationDispatcher(
    INotificationStore store,
    IUnitOfWork unitOfWork,
    INotificationSender sender,
    NotificationComposer composer,
    NotificationSettings settings,
    TimeProvider timeProvider)
{
    public Task<Guid[]> FindDueAsync(CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        return store.FindDueIdsAsync(now, now - settings.StuckAfter, settings.BatchSize, cancellationToken);
    }

    /// <summary>Returns <c>false</c> when another worker took the row first or it is no longer there.</summary>
    public async Task<bool> ProcessAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await store.FindByIdAsync(notificationId, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        // Re-checked after loading: between the worker asking which ids are due and loading one, another worker may have
        // finished it, and a finished row must not be claimed again.
        if (notification is null || !notification.IsDueAt(now, now - settings.StuckAfter) || !await ClaimAsync(notification, cancellationToken))
            return false;

        var outcome = await SendAsync(notification, cancellationToken);
        now = timeProvider.GetUtcNow().UtcDateTime;
        if (outcome.Success)
            notification.MarkSent(outcome, now);
        else
            notification.MarkFailed(outcome, now, settings.RetryDelays);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<bool> ClaimAsync(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            notification.StartProcessing();
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    /// <summary>N3: a template that is missing, off or broken, or a sender that throws, is a failed attempt, not a crash.</summary>
    private async Task<SendOutcome> SendAsync(Notification notification, CancellationToken cancellationToken)
    {
        try
        {
            var message = await composer.ComposeAsync(notification, cancellationToken);
            return await sender.SendAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new SendOutcome(false, null, null, ex.Message);
        }
    }
}
