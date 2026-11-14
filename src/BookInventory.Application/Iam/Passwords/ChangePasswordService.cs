using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// R19 and the forced change of R12: a signed-in user replaces their password. The current password is required and
/// must be right, the new one must differ from it and meet the policy, every <em>other</em> session ends
/// (<c>PASSWORD_CHANGED</c>) and the current one survives with a fresh access token - the old token still says
/// <c>pwd_change = true</c> after a forced change, so the caller needs the new one to continue.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class ChangePasswordService(
    IAuthenticationStore store,
    IPasswordHasher hasher,
    PasswordChangeService passwordChange,
    ITokenService tokenService,
    AuthSettings settings,
    ICurrentUser currentUser)
{
    public async Task<RefreshResponse> ChangeAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await store.FindUserByIdAsync(currentUser.UserId, cancellationToken)
                   ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);
        var session = await store.FindSessionAsync(currentUser.SessionId ?? Guid.Empty, cancellationToken)
                      ?? throw new UnauthenticatedException(IamMessages.SessionNotFound);

        if (hasher.Verify(request.CurrentPassword, user.PasswordHash) == PasswordVerificationResult.Failed)
            throw new ValidationFailureException(IamMessages.PasswordCurrentIncorrect);
        if (hasher.Verify(request.NewPassword, user.PasswordHash) != PasswordVerificationResult.Failed)
            throw new ValidationFailureException(IamPasswordCodes.SameAsCurrent);

        await passwordChange.SetAsync(new PasswordChange(user, request.NewPassword, LogoutReason.PasswordChanged, session.Id), cancellationToken);

        var access = tokenService.IssueAccessTokenFor(user, session.Id, settings.Tokens.AccessToken);
        return new RefreshResponse(access.Token, access.ExpiresAt, session.ExpiresAt, LoggedInUser.From(user));
    }
}
