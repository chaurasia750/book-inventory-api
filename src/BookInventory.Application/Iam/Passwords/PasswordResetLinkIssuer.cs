using System.Globalization;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BookInventory.Application.Core;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>Who a reset link is for: the three values the mail needs, taken from an account that already exists.</summary>
public sealed record ResetLinkRecipient(Guid UserId, string FullName, string Email);

/// <summary>
/// The one place a password reset link is issued, so "forgot password" (R17/R56) and the Super Admin "send a reset link"
/// action (R36) cannot drift apart: same token, same template, same variables, same invalidation of earlier tokens.
/// <para>
/// What the caller decides stays outside: <see cref="ForgotPasswordService"/> owns the rate limit of R56, because only a
/// public endpoint needs it - a Super Admin asking in front of the user is not an attack. This class only issues.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PasswordResetLinkIssuer(
    IAuthenticationStore store,
    ITokenService tokenService,
    INotificationService notifications,
    AuthSettings settings,
    ILogger<PasswordResetLinkIssuer> logger,
    TimeProvider timeProvider)
{
    /// <summary>The core notification template of R17, also seeded by <c>IamTemplateSource</c>.</summary>
    public const string TemplateCode = "PASSWORD_RESET";

    /// <summary>
    /// Issues a token for the recipient, invalidates the earlier unused ones and records the mail. The raw token exists
    /// only in the link that goes out; the row stores its hash.
    /// </summary>
    public async Task IssueAsync(ResetLinkRecipient recipient, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        foreach (var earlier in await store.FindUnusedResetTokensAsync(recipient.UserId, cancellationToken))
            earlier.Invalidate(now);

        var issued = tokenService.IssueOpaqueToken(settings.Reset.TokenLifetime);
        var token = PasswordResetToken.Issue(recipient.UserId, issued.TokenHash, issued.ExpiresAt);
        await store.AddAsync(token, cancellationToken);

        await QueueMailAsync(recipient, token, issued, cancellationToken);
    }

    /// <summary>A mail that cannot be queued (its template is missing) must not turn the answer into an error: that would tell the caller the address exists.</summary>
    private async Task QueueMailAsync(ResetLinkRecipient recipient, PasswordResetToken token, IssuedOpaqueToken issued, CancellationToken cancellationToken)
    {
        var variables = new Dictionary<string, string>
        {
            ["fullName"] = recipient.FullName,
            ["resetLink"] = $"{settings.Reset.LinkUrl}?token={Uri.EscapeDataString(issued.Token)}",
            ["expiresInMinutes"] = ((int)settings.Reset.TokenLifetime.TotalMinutes).ToString(CultureInfo.InvariantCulture)
        };

        try
        {
            await notifications.QueueAsync(
                new QueueNotification(TemplateCode, recipient.Email, variables, $"password-reset:{token.Id}"),
                cancellationToken);
        }
        catch (NotFoundException)
        {
            logger.LogError("The {Template} notification template is missing, so no reset mail was queued.", TemplateCode);
        }
    }
}
