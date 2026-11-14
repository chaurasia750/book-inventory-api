using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Auth;

/// <summary>
/// R11/D2/D3: the two cookies of a signed-in browser, both HttpOnly, Secure and <c>SameSite=Strict</c>, so page scripts can
/// never read a token and no other site can send one back.
/// <list type="bullet">
/// <item>The <b>access token</b> cookie carries the short-lived JWT for every API call (path <c>/api</c>, the life of the
/// token). The bearer authentication reads it when a request has no <c>Authorization</c> header, so a reloaded page or
/// another app is signed in without any extra call.</item>
/// <item>The <b>refresh token</b> cookie is scoped to the auth path only, so no other endpoint ever sees it.</item>
/// </list>
/// Names, paths and lifetimes come from <c>Auth:Tokens:*</c> (R54) through the settings snapshot, never from a literal here.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class SessionCookieWriter(AuthSettings settings)
{
    /// <summary>Reads the presented refresh token, or <c>null</c> when the caller has no cookie (a first login, a cleared one).</summary>
    public string? ReadRefreshToken(HttpRequest request) => request.Cookies[settings.Tokens.RefreshCookieName];

    /// <summary>A login or a refresh: rotated refresh cookie and fresh access cookie.</summary>
    public void Write(HttpResponse response, RefreshCookieGrant refresh, IAccessTokenResponse access)
    {
        WriteRefresh(response, refresh);
        WriteAccess(response, access);
    }

    /// <summary>Change-password keeps the session and the refresh token but issues a new access token.</summary>
    public void WriteAccess(HttpResponse response, IAccessTokenResponse access) =>
        response.Cookies.Append(
            settings.Tokens.AccessCookieName,
            access.AccessToken,
            Options(settings.Tokens.AccessCookiePath, Utc(access.AccessTokenExpiresAt)));

    /// <summary>
    /// Removes both cookies on logout. The path has to be the one each was written with, otherwise the browser keeps it;
    /// <see cref="CookieOptions"/> is therefore built by the same method as the writes.
    /// </summary>
    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(settings.Tokens.RefreshCookieName, Options(settings.Tokens.RefreshCookiePath, expires: null));
        response.Cookies.Delete(settings.Tokens.AccessCookieName, Options(settings.Tokens.AccessCookiePath, expires: null));
    }

    private void WriteRefresh(HttpResponse response, RefreshCookieGrant grant) =>
        response.Cookies.Append(
            settings.Tokens.RefreshCookieName,
            grant.Token,
            Options(settings.Tokens.RefreshCookiePath, Utc(grant.ExpiresAt)));

    private static DateTimeOffset Utc(DateTime value) => new(DateTime.SpecifyKind(value, DateTimeKind.Utc));

    private static CookieOptions Options(string path, DateTimeOffset? expires) =>
        new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = path,
            Expires = expires
        };
}
