using System.Linq.Expressions;
using System.Net.Mail;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Core;

/// <summary>
/// One message to one recipient, recorded before it is sent (spec core N1-N10): the outbox row of the system. It owns
/// its attempt log and every status change of the diagram in the spec; a caller can only ask for a behaviour, never
/// set a status.
/// <list type="bullet">
/// <item><b>Recording (N1, N2).</b> Created <c>PENDING</c> in the transaction of the business change; the idempotency
/// key is unique, so a repeated request finds the first row instead of adding a second.</item>
/// <item><b>Sending (N5-N7).</b> The worker claims a row with <see cref="StartProcessing"/>; the optimistic-concurrency
/// token lets only one worker win. A failure schedules the next attempt from the back-off list or ends in
/// <c>FAILED</c>.</item>
/// <item><b>Secrets (N9).</b> A notification made from a sensitive template drops its variables the moment it is
/// sent, given up or cancelled.</item>
/// </list>
/// </summary>
public class Notification : AggregateRoot
{
    public const int RecipientMaxLength = 254;
    public const string RedactedVariables = "[redacted]";

    private readonly List<NotificationLog> _logs = [];

    private Notification()
    {
        TemplateCode = string.Empty;
        Recipient = string.Empty;
        Variables = string.Empty;
        IdempotencyKey = string.Empty;
    }

    public string TemplateCode { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public NotificationPriority Priority { get; private set; }

    public string Recipient { get; private set; }

    /// <summary>The template values as JSON, or <see cref="RedactedVariables"/> once a sensitive notification is finished.</summary>
    [AuditSecret]
    public string Variables { get; private set; }

    /// <summary>Copied from the template when the notification is recorded, so a later template edit cannot expose an old secret.</summary>
    public bool IsSensitive { get; private set; }

    public NotificationStatus Status { get; private set; }

    public int RetryCount { get; private set; }

    public int MaxRetry { get; private set; }

    public DateTime? NextRetryOn { get; private set; }

    public DateTime? ScheduleOn { get; private set; }

    public DateTime? SentOn { get; private set; }

    public string? ProviderMessageId { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string? CorrelationId { get; private set; }

    public string IdempotencyKey { get; private set; }

    public IReadOnlyList<NotificationLog> Logs => _logs;

    public bool IsRedacted => Variables == RedactedVariables;

    /// <summary>
    /// N5: the one definition of "the worker should look at this row now" - pending or retry and due, or a claimed row
    /// nobody finished (the worker died) for longer than <paramref name="stuckBefore"/>.
    /// </summary>
    public static Expression<Func<Notification, bool>> IsDue(DateTime now, DateTime stuckBefore) =>
        n => !n.IsDeleted
             && ((n.Status == NotificationStatus.Pending && (n.ScheduleOn == null || n.ScheduleOn <= now))
                 || (n.Status == NotificationStatus.Retry && (n.NextRetryOn == null || n.NextRetryOn <= now))
                 || (n.Status == NotificationStatus.Processing && n.UpdatedAt != null && n.UpdatedAt <= stuckBefore));

    private static readonly Func<Notification, DateTime, DateTime, bool> DueRule = (n, now, stuckBefore) => IsDue(now, stuckBefore).Compile()(n);

    /// <summary>The same rule as <see cref="IsDue"/> for a row already in memory, so a worker re-checks it after loading.</summary>
    public bool IsDueAt(DateTime now, DateTime stuckBefore) => DueRule(this, now, stuckBefore);

    public static Notification Queue(NewNotification request, NotificationPolicy policy, DateTime utcNow)
    {
        if (!IsValidRecipient(request.Recipient))
            throw new ValidationFailureException(CoreMessages.InvalidRecipient);
        if (string.IsNullOrWhiteSpace(request.TemplateCode) || string.IsNullOrWhiteSpace(request.IdempotencyKey))
            throw new ValidationFailureException(CoreMessages.InvalidTemplate);

        return new Notification
        {
            TemplateCode = request.TemplateCode.Trim().ToUpperInvariant(),
            Channel = NotificationChannel.Email,
            Priority = policy.Priority,
            Recipient = request.Recipient.Trim(),
            Variables = request.Variables,
            IsSensitive = policy.IsSensitive,
            Status = NotificationStatus.Pending,
            MaxRetry = policy.MaxRetry,
            CorrelationId = request.CorrelationId,
            IdempotencyKey = request.IdempotencyKey.Trim(),
            ScheduleOn = utcNow
        };
    }

    /// <summary>N5: the worker takes a due row. Two workers cannot both do it: the second save hits the concurrency token.</summary>
    public void StartProcessing()
    {
        EnsureStatus(NotificationStatus.Pending, NotificationStatus.Retry, NotificationStatus.Processing);
        Status = NotificationStatus.Processing;
    }

    /// <summary>N6: the attempt worked. Logs it, closes the notification and drops a sensitive notification's variables.</summary>
    public void MarkSent(SendOutcome outcome, DateTime utcNow)
    {
        EnsureStatus(NotificationStatus.Processing);

        _logs.Add(NotificationLog.Of(Id, RetryCount + 1, NotificationStatus.Sent, outcome));
        Status = NotificationStatus.Sent;
        SentOn = utcNow;
        ProviderMessageId = outcome.ProviderMessageId;
        ErrorMessage = null;
        NextRetryOn = null;
        RedactIfSensitive();
    }

    /// <summary>
    /// N6/N7: the attempt failed. Logs it, then either schedules the next attempt from <paramref name="retryDelays"/>
    /// (the first entry is the wait after the first failure) or, when the delays or <see cref="MaxRetry"/> are used up,
    /// gives up with <c>FAILED</c>.
    /// </summary>
    public void MarkFailed(SendOutcome outcome, DateTime utcNow, IReadOnlyList<TimeSpan> retryDelays)
    {
        EnsureStatus(NotificationStatus.Processing);

        _logs.Add(NotificationLog.Of(Id, RetryCount + 1, NotificationStatus.Failed, outcome));
        ErrorMessage = outcome.Error;
        RetryCount++;

        if (RetryCount > MaxRetry || RetryCount > retryDelays.Count)
        {
            Status = NotificationStatus.Failed;
            NextRetryOn = null;
            RedactIfSensitive();
            return;
        }

        Status = NotificationStatus.Retry;
        NextRetryOn = utcNow.Add(retryDelays[RetryCount - 1]);
    }

    /// <summary>N8: stops a notification nobody has delivered yet.</summary>
    public void Cancel()
    {
        EnsureStatus(NotificationStatus.Pending, NotificationStatus.Retry);

        Status = NotificationStatus.Cancelled;
        NextRetryOn = null;
        RedactIfSensitive();
    }

    /// <summary>N7: a Super Admin sends a given-up notification again. A redacted one has nothing left to render, so it cannot be.</summary>
    public void Requeue(DateTime utcNow)
    {
        EnsureStatus(NotificationStatus.Failed);
        if (IsRedacted)
            throw new ConflictException(CoreMessages.CannotRequeueRedacted);

        Status = NotificationStatus.Pending;
        RetryCount = 0;
        ErrorMessage = null;
        NextRetryOn = null;
        ScheduleOn = utcNow;
    }

    private void RedactIfSensitive()
    {
        if (IsSensitive)
            Variables = RedactedVariables;
    }

    private void EnsureStatus(params NotificationStatus[] allowed)
    {
        if (!allowed.Contains(Status))
            throw new ConflictException(CoreMessages.InvalidTransition);
    }

    private static bool IsValidRecipient(string? value) =>
        !string.IsNullOrWhiteSpace(value)
        && value.Trim().Length <= RecipientMaxLength
        && MailAddress.TryCreate(value.Trim(), out var address)
        && address.Address == value.Trim();
}
