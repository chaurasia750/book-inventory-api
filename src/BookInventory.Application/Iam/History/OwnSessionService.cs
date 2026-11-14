using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// R15, R27: the caller's own sessions. The user id always comes from the validated token, never from the request, so a
/// user can list and end their <em>own</em> devices and nobody else's; a session id that belongs to somebody else is the same
/// 404 as one that does not exist, so ids cannot be probed. Ending one goes through <see cref="SessionEndService"/> like every
/// other way a session ends, with the reason <c>REVOKED_BY_USER</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class OwnSessionService(
    IHistoryStore history,
    IAuthenticationStore store,
    SessionEndService sessionEnd,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    public Task<OwnSessionItem[]> ListAsync(CancellationToken cancellationToken) =>
        history.FindOwnSessionsAsync(currentUser.UserId, currentUser.SessionId, Now(), cancellationToken);

    /// <summary>Ends one of the caller's sessions. Signing out the session you are using goes through <c>logout</c>, not here.</summary>
    public async Task RevokeAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await store.FindSessionAsync(sessionId, cancellationToken);
        if (session is null || session.UserId != currentUser.UserId)
            throw new NotFoundException(IamMessages.SessionNotFound);

        await sessionEnd.EndAsync(session, LogoutReason.RevokedByUser, Now(), cancellationToken);
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
