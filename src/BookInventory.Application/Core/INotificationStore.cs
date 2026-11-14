using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// The data the notification use cases need, as one persistence port (like the iam authentication store). It has no
/// save method: the request transaction commits what a use case recorded, and the worker saves through
/// <c>IUnitOfWork</c> itself.
/// </summary>
public interface INotificationStore
{
    Task<Notification?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Notification?> FindByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    Task AddAsync(Notification notification, CancellationToken cancellationToken = default);

    /// <summary>N5: the ids of the notifications the worker should look at now, oldest first (see <see cref="Notification.IsDue"/>).</summary>
    Task<Guid[]> FindDueIdsAsync(DateTime now, DateTime stuckBefore, int take, CancellationToken cancellationToken = default);

    /// <summary>The template with this code, active or not (a caller decides what an inactive one means).</summary>
    Task<NotificationTemplate?> FindTemplateAsync(string code, CancellationToken cancellationToken = default);

    Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken = default);

    /// <summary>N13: the template codes that already exist, so the seeder inserts only the missing ones.</summary>
    Task<HashSet<string>> FindTemplateCodesAsync(CancellationToken cancellationToken = default);
}
