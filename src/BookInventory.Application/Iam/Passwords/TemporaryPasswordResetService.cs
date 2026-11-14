using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>R67: the answer of a reset: the new temporary password, shown once and never retrievable again.</summary>
public sealed record TemporaryPasswordResponse(Guid UserId, string UserName, string TemporaryPassword);

/// <summary>
/// R67: a person resets the password of a company account that has no e-mail to reset by. The aggregate checks the
/// rules (company account only, not yourself, not the system account) and sets the new hash with a forced change; this
/// class records who did it, ends every session of that account and returns the new password once. Who may call it
/// (a Super Admin for any company account, an Owner for their own Staff) is decided before it is called.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class TemporaryPasswordResetService(
    IAccountStore store,
    TemporaryPasswordIssuer passwords,
    UserSessionRevoker sessions,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public async Task<TemporaryPasswordResponse> ResetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await store.FindForManagementAsync(userId, cancellationToken)
            ?? throw new NotFoundException(IamMessages.UserNotFound);
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var password = passwords.Issue();

        user.ResetToTemporaryPassword(password.Hash, currentUser.UserId, now);
        await store.AddAsync(UserPasswordReset.Record(user.Id, currentUser.UserId, now), cancellationToken);
        await sessions.RevokeAllAsync(user.Id, LogoutReason.ForcedLogout, cancellationToken);

        return new TemporaryPasswordResponse(user.Id, user.UserName, password.Plain);
    }
}
