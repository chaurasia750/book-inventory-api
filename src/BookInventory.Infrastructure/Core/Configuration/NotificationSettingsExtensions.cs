using BookInventory.Application.Core;

namespace BookInventory.Infrastructure.Core.Configuration;

/// <summary>
/// Turns the validated <c>Notification:*</c> and <c>MasterLayout:*</c> configuration into the immutable snapshot the
/// Application layer may see; the use cases never read configuration or multiply minutes themselves.
/// </summary>
public static class NotificationSettingsExtensions
{
    public static NotificationSettings ToNotificationSettings(this NotificationOptions options, MasterLayoutOptions layout) => new(
        TimeSpan.FromSeconds(options.WorkerIntervalSeconds),
        options.BatchSize,
        options.RetryMinutes.Select(minutes => TimeSpan.FromMinutes(minutes)).ToArray(),
        options.DefaultMaxRetry,
        TimeSpan.FromMinutes(options.StuckAfterMinutes),
        new MasterLayoutSettings(
            layout.CompanyName,
            layout.CompanyLogo,
            layout.CompanyWebsite,
            layout.CompanyAddress,
            layout.SupportEmail,
            layout.SupportPhone));
}
