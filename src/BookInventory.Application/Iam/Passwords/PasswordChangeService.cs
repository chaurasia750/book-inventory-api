using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>
/// The one place a password is replaced (R16, R18, R19): the policy is checked, the new password is hashed and set on the
/// aggregate (which clears the forced-change flag and the lockout), and every session of the user - except the one the
/// caller is using, on a change - ends with the reason that matches. Change-password and reset-password both come
/// through here, so the policy and the revocation can never differ between them.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PasswordChangeService(
    IAuthenticationStore store,
    IPasswordHasher hasher,
    SessionEndService sessionEnd,
    AuthSettings settings,
    TimeProvider timeProvider)
{
    public async Task SetAsync(PasswordChange change, CancellationToken cancellationToken)
    {
        var failures = settings.Password.Validate(change.NewPassword);
        if (failures.Count > 0)
            throw new ValidationFailureException(failures[0]);

        var now = timeProvider.GetUtcNow().UtcDateTime;
        change.User.SetPassword(hasher.Hash(change.NewPassword), now);

        var sessions = await store.FindActiveSessionsAsync(change.User.Id, cancellationToken);
        foreach (var session in sessions.Where(s => s.Id != change.KeepSessionId))
            await sessionEnd.EndAsync(session, change.Reason, now, cancellationToken);
    }
}
