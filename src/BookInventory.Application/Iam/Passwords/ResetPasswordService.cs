using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// R17, R18: the two steps behind the mailed link. <see cref="VerifyAsync"/> only says whether the link can still be
/// used, so the page can show the form or an "expired" message; <see cref="ResetAsync"/> sets the new password (which
/// clears the forced change and the lockout), marks the token used so a second try fails, and ends <em>every</em> session
/// of the user (<c>PASSWORD_RESET</c>) - whoever held the old password or a stolen session is signed out.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ResetPasswordService(
    IAuthenticationStore store,
    ITokenService tokenService,
    PasswordChangeService passwordChange,
    TimeProvider timeProvider)
{
    public async Task<VerifyResetTokenResponse> VerifyAsync(string? token, CancellationToken cancellationToken)
    {
        var (resetToken, user) = await FindAsync(token, cancellationToken);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        return new VerifyResetTokenResponse(
            resetToken is { IsUsed: false } && resetToken.ExpiresAt > now && user is { Status: AccountStatus.Active });
    }

    public async Task ResetAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var (resetToken, user) = await FindAsync(request.Token, cancellationToken);
        if (resetToken is null || user is not { Status: AccountStatus.Active })
            throw new ValidationFailureException(IamMessages.TokenNotFound);

        resetToken.EnsureUsableAt(timeProvider.GetUtcNow().UtcDateTime);

        await passwordChange.SetAsync(new PasswordChange(user, request.NewPassword, LogoutReason.PasswordReset), cancellationToken);
        resetToken.MarkUsed(timeProvider.GetUtcNow().UtcDateTime);
    }

    private async Task<(PasswordResetToken? Token, AppUser? User)> FindAsync(string? token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (null, null);

        var resetToken = await store.FindResetTokenAsync(tokenService.HashOpaqueToken(token.Trim()), cancellationToken);
        var user = resetToken is null ? null : await store.FindUserByIdAsync(resetToken.UserId, cancellationToken);
        return (resetToken, user);
    }
}
