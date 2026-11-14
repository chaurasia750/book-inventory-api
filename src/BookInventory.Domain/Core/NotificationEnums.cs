namespace BookInventory.Domain.Core;

/// <summary>How a notification reaches the person. Only <see cref="Email"/> is built; the others are reserved (spec core CO2).</summary>
public enum NotificationChannel
{
    Email,
    Sms,
    WhatsApp,
    Push
}

public enum NotificationPriority
{
    High,
    Normal,
    Low
}

/// <summary>Lifecycle of a notification (spec core section 2). Every change goes through <see cref="Notification"/>.</summary>
public enum NotificationStatus
{
    Pending,
    Processing,
    Sent,
    Retry,
    Failed,
    Cancelled
}
