using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// R11: the login input. Mobile is deliberately not accepted (R7) and is not a member of this record. The bounds are
/// data annotations so the API answers 400 with a <c>ValidationProblemDetails</c> before a use case ever sees a
/// missing or oversized value.
/// </summary>
public sealed record LoginRequest(
    [param: Required, StringLength(256, MinimumLength = 1)] string Identifier,
    [param: Required, StringLength(256, MinimumLength = 1)] string Password,
    bool RememberMe = false);

/// <summary>
/// R22/R23: what the caller of one attempt is. The real client IP is already resolved when this is built - forwarded
/// headers are only honoured from trusted proxies, so a forged <c>X-Forwarded-For</c> cannot change it.
/// </summary>
public sealed record ClientContext(string? IpAddress, string? UserAgent, string? AcceptLanguage, string? TraceId);

/// <summary>What one login attempt is: the request and the client it came from (at most four parameters per method).</summary>
public sealed record LoginCommand(LoginRequest Login, ClientContext Client);

/// <summary>
/// One login attempt reduced to what the log and the session need: the identifier as it was typed (the aggregate
/// masks it when it matched no account), the client it came from and whether "remember me" was asked for.
/// </summary>
public sealed record LoginContext(string Identifier, ClientContext Client, bool RememberMe);

/// <summary>
/// Who is signed in (R11, R12, R27): the one shape login, refresh and <c>GET auth/me</c> all return, so the UI keeps a
/// single user model. Never another user's profile, and never a credential.
/// </summary>
public sealed record LoggedInUser(
    Guid Id,
    string UserName,
    string FullName,
    string? Email,
    string? Mobile,
    Role Role,
    bool MustChangePassword,
    DateTime? LastLoginAt)
{
    public static LoggedInUser From(AppUser user) => new(
        user.Id,
        user.UserName,
        user.FullName,
        user.Email,
        user.Mobile,
        user.Role,
        user.MustChangePassword,
        user.LastLoginAt);
}
