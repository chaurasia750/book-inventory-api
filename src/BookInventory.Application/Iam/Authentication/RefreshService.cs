using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R11/R13: the refresh use case. It finds the session behind the cookie, rotates the refresh token and issues a new
/// access token. Presenting a token that is not the session's current one means it was already used, so the session is
/// revoked and the attempt is logged - and that revoke has to survive the 401, so this use case also returns instead
/// of throwing and its action is marked <c>[PersistOnFailure]</c>.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class RefreshService(
    IAuthenticationStore store,
    ITokenService tokenService,
    SessionEndService sessionEnd,
    AuthSettings settings,
    TimeProvider timeProvider)
{
    private static AuthOutcome<IssuedSession<RefreshResponse>> Denied(string code) =>
        AuthOutcome<IssuedSession<RefreshResponse>>.Denied(code);

    public async Task<AuthOutcome<IssuedSession<RefreshResponse>>> RefreshAsync(
        string? refreshToken,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var presentedHash = tokenService.HashOpaqueToken(refreshToken ?? string.Empty);
        var session = await store.FindSessionByPresentedTokenAsync(presentedHash, cancellationToken);

        if (session is null)
            return Denied(IamMessages.SessionNotFound);
        if (session.IsRevoked)
            return Denied(IamMessages.SessionRevoked);

        var expired = await CloseIfExpiredAsync(session, now, cancellationToken);
        if (expired is not null)
            return Denied(expired);

        var user = await store.FindUserByIdAsync(session.UserId, cancellationToken);
        var ended = await CloseIfAccountEndedAsync(session, user, now, cancellationToken);
        if (ended is not null)
            return Denied(ended);

        var (refreshLifetime, cookieLifetime) = settings.Tokens.LifetimesForRotation(user!, session, now);
        var refresh = tokenService.IssueOpaqueToken(refreshLifetime);
        if (!session.Rotate(presentedHash, refresh.TokenHash, refresh.ExpiresAt, now))
            return await RefuseReuseAsync(session, now, cancellationToken);

        return IssueTokens(session, user!, refresh, TokenSettings.CookieExpiry(now, cookieLifetime, refresh.ExpiresAt));
    }

    /// <summary>
    /// R13: the presented token is not the current one, so somebody is replaying a token that already worked. The
    /// aggregate has already revoked the session; the row is closed with <c>REFRESH_REUSE</c> and answered 401.
    /// </summary>
    private async Task<AuthOutcome<IssuedSession<RefreshResponse>>> RefuseReuseAsync(
        UserSession session,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await sessionEnd.EndAsync(session, LogoutReason.RefreshReuse, now, cancellationToken);
        return Denied(IamMessages.SessionRefreshReuse);
    }

    /// <summary>
    /// R24/R13: a session that ran out - or reached its absolute life - is closed here, lazily, with
    /// <c>logged_out_at</c> at the moment it ended.
    /// </summary>
    private async Task<string?> CloseIfExpiredAsync(UserSession session, DateTime now, CancellationToken cancellationToken)
    {
        var end = session.ExpiresAt < settings.Tokens.AbsoluteEnd(session) ? session.ExpiresAt : settings.Tokens.AbsoluteEnd(session);
        if (end > now)
            return null;

        await sessionEnd.EndAsync(session, LogoutReason.Expired, end, cancellationToken);
        return IamMessages.SessionExpired;
    }

    /// <summary>
    /// R31 groundwork: a session of an account that is no longer active dies with the reason of R24 that matches the
    /// status. The status handlers of the account module do the same thing when they revoke, so this is the second
    /// line of defence, not a second rule.
    /// </summary>
    private async Task<string?> CloseIfAccountEndedAsync(
        UserSession session,
        AppUser? user,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (user is null)
            return IamMessages.SessionRevoked;
        if (user.Status == AccountStatus.Active)
            return null;

        var reason = user.Status == AccountStatus.Suspended
            ? LogoutReason.AccountSuspended
            : LogoutReason.AccountDeactivated;

        await sessionEnd.EndAsync(session, reason, now, cancellationToken);
        return IamMessages.SessionRevoked;
    }

    /// <summary>R11: a new access token for the same session, plus the rotated cookie.</summary>
    private AuthOutcome<IssuedSession<RefreshResponse>> IssueTokens(
        UserSession session,
        AppUser user,
        IssuedOpaqueToken refresh,
        DateTime cookieExpiresAt)
    {
        var access = tokenService.IssueAccessTokenFor(user, session, settings.Tokens.AccessToken);

        var body = new RefreshResponse(access.Token, access.ExpiresAt, session.ExpiresAt, LoggedInUser.From(user));
        return AuthOutcome<IssuedSession<RefreshResponse>>.GrantedWith(
            new IssuedSession<RefreshResponse>(body, new RefreshCookieGrant(refresh.Token, cookieExpiresAt)));
    }
}
