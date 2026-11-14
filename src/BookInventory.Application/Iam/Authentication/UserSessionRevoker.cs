using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R31/R24: ends <em>every</em> session of one user because the account itself changed - it was deactivated, suspended
/// or moved to another role - and a force logout (R36). It goes through <see cref="SessionEndService"/>, so each session
/// is revoked with the right reason, its login-log row is closed with the same reason and the cached "is this session
/// alive?" answer is dropped, which is what makes the next request of that user a 401.
/// <para>
/// It is a plain use-case collaborator rather than a domain-event handler on purpose: handlers are dispatched
/// <em>after</em> the save of the request transaction, so anything they change is never written.
/// Calling it from the use case that performs the change keeps the status change, the history row and the
/// session revocations in one transaction.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserSessionRevoker(
    IAuthenticationStore store,
    SessionEndService sessionEnd,
    TimeProvider timeProvider)
{
    /// <summary>Revokes every usable session of the user; a user without sessions is not an error.</summary>
    public async Task RevokeAllAsync(Guid userId, LogoutReason reason, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;

        foreach (var session in await store.FindActiveSessionsAsync(userId, cancellationToken))
            await sessionEnd.EndAsync(session, reason, now, cancellationToken);
    }

    /// <summary>
    /// Revokes one named session, for the admin's sessions tab (R39). A session that does not exist and a session of
    /// somebody else are the same 404, so a Super Admin cannot probe another user's session ids.
    /// </summary>
    public async Task RevokeOneAsync(Guid userId, Guid sessionId, LogoutReason reason, CancellationToken cancellationToken)
    {
        var session = await store.FindSessionAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != userId)
            throw new NotFoundException(IamMessages.SessionNotFound);

        await sessionEnd.EndAsync(session, reason, timeProvider.GetUtcNow().UtcDateTime, cancellationToken);
    }
}
