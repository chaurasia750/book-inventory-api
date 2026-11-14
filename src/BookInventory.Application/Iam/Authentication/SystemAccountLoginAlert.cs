using System.Globalization;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BookInventory.Application.Core;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R50: every successful login of the hidden system account leaves a security warning in the application log and, when
/// <c>SystemAccount:AlertEmail</c> is set, a mail through the core outbox. The account is invisible in every screen, so
/// this is how the team notices that somebody used it.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SystemAccountLoginAlert(
    SystemAccountSettings settings,
    INotificationService notifications,
    ILogger<SystemAccountLoginAlert> logger,
    TimeProvider timeProvider)
{
    /// <summary>The core notification template of the alert, also seeded by <c>IamTemplateSource</c>.</summary>
    public const string TemplateCode = "SYSTEM_ACCOUNT_LOGIN";

    public async Task RaiseAsync(Guid sessionId, ClientContext client, CancellationToken cancellationToken)
    {
        logger.LogWarning(
            "Security: the hidden system account signed in from {IpAddress} (session {SessionId}).",
            client.IpAddress,
            sessionId);

        if (settings.AlertEmail is null)
            return;

        var variables = new Dictionary<string, string>
        {
            ["signedInAt"] = timeProvider.GetUtcNow().UtcDateTime.ToString("u", CultureInfo.InvariantCulture),
            ["ipAddress"] = client.IpAddress ?? "unknown",
            ["userAgent"] = client.UserAgent ?? "unknown"
        };

        try
        {
            await notifications.QueueAsync(
                new QueueNotification(TemplateCode, settings.AlertEmail, variables, $"system-login:{sessionId}"),
                cancellationToken);
        }
        catch (NotFoundException)
        {
            logger.LogError("The {Template} notification template is missing, so no alert mail was queued.", TemplateCode);
        }
    }
}
