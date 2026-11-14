using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// R17, R56: forgot password. It always looks the same from outside: the caller gets 202 whether the address exists,
/// belongs to an active account, was asked for too often, or the mail could not even be queued. Only an <c>ACTIVE</c>
/// account gets a link; who builds the link is <see cref="PasswordResetLinkIssuer"/>, shared with the Super Admin action.
/// <para>
/// The per-hour limit of R56 lives here and not in the issuer, because only a public endpoint needs it.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ForgotPasswordService(
    IAuthenticationStore store,
    PasswordResetLinkIssuer links,
    AuthSettings settings,
    TimeProvider timeProvider)
{
    public async Task RequestAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var email = IdentityNormalizer.NormalizeEmail(request.Email);
        var user = email is null ? null : await store.FindUserByEmailAsync(email, cancellationToken);
        if (user is not { Status: AccountStatus.Active } || user.Email is null)
            return;

        var anHourAgo = timeProvider.GetUtcNow().UtcDateTime.AddHours(-1);
        if (await store.CountResetTokensSinceAsync(user.Id, anHourAgo, cancellationToken) >= settings.Reset.MaxRequestsPerHour)
            return;

        await links.IssueAsync(new ResetLinkRecipient(user.Id, user.FullName, user.Email), cancellationToken);
    }
}
