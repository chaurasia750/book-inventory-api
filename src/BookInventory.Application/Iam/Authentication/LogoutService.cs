using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R14/R24/R27: logout and logout-all. Both revoke sessions of the <em>caller</em> - the id comes from the validated
/// token, never from the request - and both close the login-log row of every session they end, so the history always
/// says why a session died. Revoking an already revoked session is not an error: signing out twice is normal.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LogoutService(
    IAuthenticationStore store,
    SessionEndService sessionEnd,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    /// <summary>R14: revokes the session of the current access token and closes its log row as <c>USER_LOGOUT</c>.</summary>
    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        var sessionId = currentUser.SessionId ?? Guid.Empty;
        var session = sessionId == Guid.Empty
            ? null
            : await store.FindSessionAsync(sessionId, cancellationToken);

        if (session is null || session.UserId != currentUser.UserId)
            return;

        await sessionEnd.EndAsync(session, LogoutReason.UserLogout, Now(), cancellationToken);
    }

    /// <summary>R14: revokes every session of the user, including the current one, each with <c>LOGOUT_ALL</c>.</summary>
    public async Task LogoutAllAsync(CancellationToken cancellationToken)
    {
        var now = Now();
        var sessions = await store.FindActiveSessionsAsync(currentUser.UserId, cancellationToken);

        foreach (var session in sessions)
            await sessionEnd.EndAsync(session, LogoutReason.LogoutAll, now, cancellationToken);
    }

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
