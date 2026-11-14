namespace BookInventory.Application.Iam.Authentication;

/// <summary>Common shape of a response that carries a freshly issued access token, so the API can mirror it into the HttpOnly access cookie (spec platform P16/U2).</summary>
public interface IAccessTokenResponse
{
    string AccessToken { get; }
    DateTime AccessTokenExpiresAt { get; }
}

/// <summary>
/// R11/R13: the body of a successful login. The access token is returned in the body (the UI keeps it in memory only);
/// the refresh token never appears here - it goes into the HttpOnly cookie as <see cref="IssuedSession{T}.Cookie"/>.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    LoggedInUser User) : IAccessTokenResponse;

/// <summary>R11/R13: the body of a successful refresh - a new access token, the user it belongs to (so a reloaded page learns who is signed in) and the fact that the cookie was rotated.</summary>
public sealed record RefreshResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    LoggedInUser User) : IAccessTokenResponse;

/// <summary>The rotating refresh token and when the cookie expires (D3). Never logged, never in a response body.</summary>
public sealed record RefreshCookieGrant(string Token, DateTime ExpiresAt);

/// <summary>
/// What a successful login or refresh produced: the JSON body plus the cookie to write. The two are separated so the
/// refresh token cannot leak into the body by accident.
/// </summary>
public sealed record IssuedSession<TResponse>(TResponse Response, RefreshCookieGrant Cookie);
