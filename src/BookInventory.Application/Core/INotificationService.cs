namespace BookInventory.Application.Core;

/// <summary>
/// The one way a module sends anything to a person (spec core N1): it records the notification in the caller's own
/// transaction and returns; the worker sends it later. Nothing here talks to a mail server.
/// </summary>
public interface INotificationService
{
    /// <summary>N1/N2: records a <c>PENDING</c> notification, or returns the one an earlier request with the same key made.</summary>
    Task<QueuedNotification> QueueAsync(QueueNotification request, CancellationToken cancellationToken = default);

    /// <summary>N8: stops a notification nobody has delivered yet.</summary>
    Task CancelAsync(Guid notificationId, CancellationToken cancellationToken = default);

    /// <summary>N7: sends a given-up notification again.</summary>
    Task RequeueAsync(Guid notificationId, CancellationToken cancellationToken = default);
}
