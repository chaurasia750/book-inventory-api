using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R11: the successful end of a login - the session, the stamped account, the one success row and the two tokens. Split
/// from <see cref="LoginService"/> so that use case keeps only the flow of the attempt and stays free of a dependency on
/// the token lifetimes.
/// <para>
/// The refresh token is returned for the cookie, never in the body (R23).
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LoginGrantService(
    IAuthenticationStore store,
    ITokenService tokenService,
    LoginLogService loginLog,
    SystemAccountLoginAlert systemAlert,
    AuthSettings settings)
{
    public async Task<AuthOutcome<IssuedSession<LoginResponse>>> GrantAsync(
        AppUser user,
        LoginContext context,
        DateTime now,
        CancellationToken cancellationToken)
    {
        var rememberMe = context.RememberMe && !user.IsSystemAccount;
        var (refreshLifetime, cookieLifetime) = settings.Tokens.LifetimesFor(user, rememberMe);
        var refresh = tokenService.IssueOpaqueToken(refreshLifetime);
        var client = new ClientInfo(context.Client.IpAddress, context.Client.UserAgent);

        var session = UserSession.Open(user.Id, new SessionGrant(refresh.TokenHash, refresh.ExpiresAt, rememberMe), client, now);
        await store.AddAsync(session, cancellationToken);

        user.RecordSuccessfulLogin(now);
        await loginLog.RecordSuccessAsync(user, session, context, cancellationToken);
        if (user.IsSystemAccount)
            await systemAlert.RaiseAsync(session.Id, context.Client, cancellationToken);

        var access = tokenService.IssueAccessTokenFor(user, session, settings.Tokens.AccessToken);

        var body = new LoginResponse(access.Token, access.ExpiresAt, refresh.ExpiresAt, LoggedInUser.From(user));

        return AuthOutcome<IssuedSession<LoginResponse>>.GrantedWith(
            new IssuedSession<LoginResponse>(body, new RefreshCookieGrant(refresh.Token, TokenSettings.CookieExpiry(now, cookieLifetime, refresh.ExpiresAt))));
    }
}
