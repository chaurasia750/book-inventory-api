using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// The three Super Admin actions on a live account that are not a status change (Plan 0006, task 3): clear a lockout
/// (R34), force a logout (R36) and force a password change on the next login (R36).
/// <para>
/// Unlock and force-password-change <em>change</em> the account, so they load it through <see cref="IAccountStore"/> and
/// the aggregate refuses the hidden system account with 403. Force-logout only ends sessions, and it is answered from
/// the visible directory, so the system account id is a 404 like any other unknown id - a decision of the owner, because
/// a logout of an account that cannot be logged out should not confirm that the id exists.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserAccessService(
    IAccountStore store,
    IUserDirectory directory,
    UserSessionRevoker sessions,
    ICurrentUser currentUser)
{
    /// <summary>
    /// R34: clears <c>failed_login_count</c> and <c>locked_until</c> before the lockout expires. The aggregate's audit
    /// columns are the record of who unlocked and when; the status is untouched, so no status history row is written.
    /// </summary>
    public async Task UnlockAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        user.Unlock(currentUser.UserId);
    }

    /// <summary>
    /// R36: revokes every session of the user with reason <c>FORCED_LOGOUT</c>, so the next request of that user is a 401
    /// and the login-log rows are closed with the same reason. A user without a session is not an error.
    /// </summary>
    public async Task ForceLogoutAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await directory.GetVisibleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(IamMessages.UserNotFound);

        await sessions.RevokeAllAsync(user.Id, LogoutReason.ForcedLogout, cancellationToken);
    }

    /// <summary>
    /// R36: the user has to choose a new password at the next login. The sessions are deliberately left alone - the
    /// user is not thrown out, their password is simply not enough any more once they log in again.
    /// </summary>
    public async Task ForcePasswordChangeAsync(Guid id, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        user.RequirePasswordChange();
    }

    /// <summary>R39: ends one session from the admin's sessions tab, leaving the user's other sessions alone.</summary>
    public async Task RevokeSessionAsync(Guid id, Guid sessionId, CancellationToken cancellationToken)
    {
        var user = await directory.GetVisibleByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(IamMessages.UserNotFound);

        await sessions.RevokeOneAsync(user.Id, sessionId, LogoutReason.RevokedByAdmin, cancellationToken);
    }

    private async Task<AppUser> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken)
        ?? throw new NotFoundException(IamMessages.UserNotFound);
}
