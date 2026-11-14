namespace BookInventory.Domain.Iam;

/// <summary>A suspension: why, and optionally when it ends (R30).</summary>
public sealed record SuspensionRequest(string Reason, DateTime? Until = null);

/// <summary>A role change between staff roles: the new role and the mandatory reason (R6).</summary>
public sealed record RoleChange(Role NewRole, string Reason);

/// <summary>One transition of the account status, as written to the status history (R33).</summary>
public sealed record StatusTransition(AccountStatus From, AccountStatus To, string? Reason, DateTime? Until = null);

/// <summary>One transition of the role, as written to the role history (R6).</summary>
public sealed record RoleTransition(Role From, Role To, string? Reason);

/// <summary>What a session or a login attempt knows about the caller's device (R22, R23).</summary>
public sealed record ClientInfo(string? IpAddress, string? UserAgent);

/// <summary>
/// The refresh token hash a session holds and when it stops being usable (R11). <c>RememberMe</c> is kept on the session
/// because a rotation has to know which lifetime to extend by (12 hours, or 30 days), and the cookie itself carries
/// nothing but the token.
/// </summary>
public sealed record SessionGrant(string RefreshTokenHash, DateTime ExpiresAt, bool RememberMe = false);

/// <summary>Everything recorded about one login attempt besides who it was and what came out of it (R20, R22).</summary>
public sealed record LoginAttempt(
    string IdentifierAttempted,
    DateTime LoginAt,
    ClientInfo Client,
    string? AcceptLanguage,
    bool IsNewDevice,
    bool IsNewIp,
    string? TraceId);

/// <summary>The parsed user agent of a login attempt (R22); the parsing itself is the caller's job (D11).</summary>
public sealed record ClientDevice(string? BrowserName, string? BrowserVersion, string? OsName, string? OsVersion, DeviceType DeviceType);
