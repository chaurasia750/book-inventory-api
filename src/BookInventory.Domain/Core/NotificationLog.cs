using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Core;

/// <summary>
/// One send attempt of one notification (spec core N6). A child of <see cref="Notification"/>, added only by it, and
/// never edited or deleted afterwards.
/// </summary>
public class NotificationLog : TransactionDomainBase
{
    private NotificationLog() => Response = string.Empty;

    public Guid NotificationId { get; private set; }

    public int AttemptNo { get; private set; }

    public NotificationStatus Status { get; private set; }

    public string Response { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? ProviderMessageId { get; private set; }

    internal static NotificationLog Of(Guid notificationId, int attemptNo, NotificationStatus status, SendOutcome outcome) =>
        new()
        {
            NotificationId = notificationId,
            AttemptNo = attemptNo,
            Status = status,
            Response = outcome.Response ?? string.Empty,
            ErrorMessage = outcome.Error,
            ProviderMessageId = outcome.ProviderMessageId
        };
}
