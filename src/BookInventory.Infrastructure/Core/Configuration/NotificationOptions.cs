using System.ComponentModel.DataAnnotations;

namespace BookInventory.Infrastructure.Core.Configuration;

/// <summary>
/// The worker and retry settings of the notification outbox (spec core N5, N7), bound from <c>Notification:*</c> and
/// validated at startup: a retry list with a zero or an empty list stops the application instead of retrying in a tight loop.
/// </summary>
public sealed class NotificationOptions : IValidatableObject
{
    public const string SectionName = "Notification";

    [Range(1, 3600)]
    public int WorkerIntervalSeconds { get; set; } = 30;

    [Range(1, 500)]
    public int BatchSize { get; set; } = 50;

    /// <summary>The wait after the first, second, ... failed attempt, in minutes; when the list is used up the notification fails.</summary>
    public int[] RetryMinutes { get; set; } = [5, 15, 30, 60, 360];

    [Range(0, 20)]
    public int DefaultMaxRetry { get; set; } = 5;

    /// <summary>A notification claimed by a worker that then died is picked up again after this long.</summary>
    [Range(1, 1440)]
    public int StuckAfterMinutes { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RetryMinutes.Length == 0 || RetryMinutes.Any(minutes => minutes is < 1 or > 10080))
            yield return new ValidationResult(
                "Notification:RetryMinutes needs at least one entry and every entry must be between 1 and 10080 minutes.",
                [nameof(RetryMinutes)]);
    }
}
