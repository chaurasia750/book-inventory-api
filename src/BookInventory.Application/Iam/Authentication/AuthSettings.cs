using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// The security values the authentication use cases need (R54), as one immutable snapshot. Configuration stays in
/// <c>AuthOptions</c> (Infrastructure, bound and validated at startup); this record is what the Application layer is
/// allowed to see, so no use case ever reads configuration or a magic number.
/// </summary>
public sealed record AuthSettings(LockoutPolicy Lockout, PasswordPolicy Password, TokenSettings Tokens, ResetSettings Reset);

/// <summary>The password reset link of R17: how long it lives, how often one user may ask for one, and where the link points.</summary>
public sealed record ResetSettings(TimeSpan TokenLifetime, int MaxRequestsPerHour, string LinkUrl);

/// <summary>
/// The token lifetimes and cookie settings of R11/D3. The lifetimes are <see cref="TimeSpan"/>s, not minutes, so a
/// use case never multiplies anything.
/// </summary>
public sealed record TokenSettings(
    TimeSpan AccessToken,
    TimeSpan RefreshToken,
    TimeSpan RememberMe,
    TimeSpan RefreshCookie,
    TimeSpan SystemAccountSession,
    TimeSpan AbsoluteSession,
    string RefreshCookieName,
    string RefreshCookiePath,
    string AccessCookieName,
    string AccessCookiePath)
{
    /// <summary>
    /// R11: 12 hours by default, 30 days with "remember me". The cookie follows the grant, except that an ordinary
    /// login is additionally capped by <see cref="RefreshCookie"/> so a cookie can never outlive its own setting.
    /// </summary>
    public (TimeSpan Refresh, TimeSpan Cookie) LifetimesFor(bool rememberMe) => rememberMe
        ? (RememberMe, RememberMe)
        : (RefreshToken, RefreshToken <= RefreshCookie ? RefreshToken : RefreshCookie);

    /// <summary>
    /// R50: the system account's session lasts <see cref="SystemAccountSession"/> and cannot be remembered; every other
    /// account follows <see cref="LifetimesFor(bool)"/>.
    /// </summary>
    public (TimeSpan Refresh, TimeSpan Cookie) LifetimesFor(AppUser user, bool rememberMe) =>
        user.IsSystemAccount ? (SystemAccountSession, SystemAccountSession) : LifetimesFor(rememberMe);

    /// <summary>
    /// R11/R13/R50: the lifetime of a rotated token. An ordinary session slides, but never past <see cref="AbsoluteEnd"/>; the system account's session is hard-capped
    /// (R50), so its rotated token ends where the session already ends and refresh can never extend it past the cap.
    /// </summary>
    public (TimeSpan Refresh, TimeSpan Cookie) LifetimesForRotation(AppUser user, UserSession session, DateTime now)
    {
        if (!user.IsSystemAccount)
        {
            var (refresh, cookie) = LifetimesFor(session.IsRememberMe);
            var untilCap = AbsoluteEnd(session) - now;
            return (refresh < untilCap ? refresh : untilCap, cookie < untilCap ? cookie : untilCap);
        }

        var remaining = session.ExpiresAt - now;
        return (remaining, remaining);
    }

    /// <summary>R13: when a session ends for good, however often it was refreshed (owner decision, Plan 0014).</summary>
    public DateTime AbsoluteEnd(UserSession session) => session.CreatedAt + AbsoluteSession;

    /// <summary>R11: the cookie never outlives the token it carries, nor its own lifetime, on a login as on every rotation.</summary>
    public static DateTime CookieExpiry(DateTime now, TimeSpan cookieLifetime, DateTime refreshTokenExpiresAt)
    {
        var cookieEnd = now.Add(cookieLifetime);
        return cookieEnd < refreshTokenExpiresAt ? cookieEnd : refreshTokenExpiresAt;
    }
}
