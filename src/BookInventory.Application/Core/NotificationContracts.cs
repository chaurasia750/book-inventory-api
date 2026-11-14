using BookInventory.Domain.Core;

namespace BookInventory.Application.Core;

/// <summary>
/// What another module hands to <see cref="INotificationService"/> to have somebody told (spec core N2, section 5). The
/// idempotency key makes the request repeatable: the same key returns the first notification instead of adding one.
/// </summary>
public sealed record QueueNotification(
    string TemplateCode,
    string Recipient,
    IReadOnlyDictionary<string, string> Variables,
    string IdempotencyKey,
    string? CorrelationId = null);

/// <summary>The notification that was recorded (or that an earlier identical request had already recorded).</summary>
public sealed record QueuedNotification(Guid Id, NotificationStatus Status);

/// <summary>One email ready for a sender; the sender adds the from address and display name it is configured with.</summary>
public sealed record EmailMessage(string To, string Subject, string Body, bool IsHtml);

/// <summary>The values of the worker and the retry policy, as one immutable snapshot the Application layer may see (spec N5, N7).</summary>
public sealed record NotificationSettings(
    TimeSpan WorkerInterval,
    int BatchSize,
    IReadOnlyList<TimeSpan> RetryDelays,
    int DefaultMaxRetry,
    TimeSpan StuckAfter,
    MasterLayoutSettings Layout);

/// <summary>The company details the master layout shows around every HTML email (spec N4).</summary>
public sealed record MasterLayoutSettings(
    string CompanyName,
    string CompanyLogo,
    string CompanyWebsite,
    string CompanyAddress,
    string SupportEmail,
    string SupportPhone);
