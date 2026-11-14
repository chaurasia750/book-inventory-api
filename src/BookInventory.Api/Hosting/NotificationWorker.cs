using Microsoft.Extensions.Options;
using BookInventory.Application.Core;
using BookInventory.Infrastructure.Core.Configuration;

namespace BookInventory.Api.Hosting;

/// <summary>
/// Spec core N5: every <c>Notification:WorkerIntervalSeconds</c> the worker asks which notifications are due and sends
/// them one by one, each in a scope of its own (a broken notification can neither poison the next one's change tracker nor
/// stop the batch). A failure of a tick is logged and the next tick tries again; nothing about the requests being served
/// depends on this loop. The sending itself - claim, compose, send, log, retry - is
/// <see cref="NotificationDispatcher"/>, so this class only schedules.
/// </summary>
public sealed class NotificationWorker(
    IServiceScopeFactory scopeFactory,
    NotificationSettings settings,
    IOptions<EmailOptions> email,
    ILogger<NotificationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!email.Value.HasProvider)
            logger.LogWarning("Email:Host is not set: notifications are recorded and marked sent, but NO real email is delivered (logging sender). Set Email:* before production.");

        using var timer = new PeriodicTimer(settings.WorkerInterval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await TickAsync(stoppingToken);
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        try
        {
            var due = await FindDueAsync(cancellationToken);
            foreach (var id in due)
                await ProcessAsync(id, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The notification worker tick failed and will run again on the next interval.");
        }
    }

    private async Task<Guid[]> FindDueAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<NotificationDispatcher>().FindDueAsync(cancellationToken);
    }

    private async Task ProcessAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<NotificationDispatcher>().ProcessAsync(notificationId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Notification {NotificationId} could not be processed; it stays queued for the next interval.", notificationId);
        }
    }
}
