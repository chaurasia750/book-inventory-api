using System.Net;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// One row per login attempt, success or failure, including unknown identifiers and locked accounts (R20, R22). It is
/// its own root because a failed attempt has no user to hang it under. Apart from the one-time logout columns the row
/// is never edited and never deleted (R25). The class is named after the ERD table <c>user_login_log</c>.
/// <list type="bullet">
/// <item><see cref="UserId"/> is null when the identifier matched nothing, so the row records only the masked
/// identifier that was tried - never a password.</item>
/// <item><see cref="Close"/> fills <c>logged_out_at</c> / <c>logout_reason</c> exactly once, which is why
/// <see cref="LogoutReason"/> is shared with <see cref="UserSession"/>.</item>
/// <item><see cref="IsNewDevice"/> / <see cref="IsNewIp"/> are stored per row so a later alert is possible without
/// re-parsing history.</item>
/// </list>
/// </summary>
public class UserLoginLog : AggregateRoot
{
    private UserLoginLog() => IdentifierAttempted = string.Empty;

    public Guid? UserId { get; private set; }

    public Guid? SessionId { get; private set; }

    /// <summary>What the caller typed, masked when it matched no account. Never the password (R22).</summary>
    public string IdentifierAttempted { get; private set; }

    public LoginResult Result { get; private set; }

    public DateTime LoginAt { get; private set; }

    public DateTime? LoggedOutAt { get; private set; }

    public LogoutReason? LogoutReason { get; private set; }

    /// <summary>The real caller IP (R23): forwarded headers only from trusted proxies. <c>inet</c> in PostgreSQL.</summary>
    public IPAddress? IpAddress { get; private set; }

    /// <summary>Raw user agent, kept as sent so a parser upgrade can re-read history.</summary>
    public string? UserAgent { get; private set; }

    public string? BrowserName { get; private set; }

    public string? BrowserVersion { get; private set; }

    public string? OsName { get; private set; }

    public string? OsVersion { get; private set; }

    public DeviceType DeviceType { get; private set; }

    public string? AcceptLanguage { get; private set; }

    public bool IsNewDevice { get; private set; }

    public bool IsNewIp { get; private set; }

    public string? TraceId { get; private set; }

    public bool IsClosed => LoggedOutAt is not null;

    /// <summary>Records a successful login. The session link is set here, once it exists.</summary>
    public static UserLoginLog Success(Guid userId, Guid sessionId, LoginAttempt attempt) =>
        Create(userId, sessionId, LoginResult.Success, attempt);

    /// <summary>
    /// Records a failed attempt (R21: this row and the failed-login counter are committed even though the request
    /// answers 401/423). <paramref name="userId"/> is null for an unknown identifier.
    /// </summary>
    public static UserLoginLog Failure(Guid? userId, LoginResult result, LoginAttempt attempt) =>
        Create(userId, sessionId: null, result, attempt);

    /// <summary>
    /// R24: fills the logout columns exactly once. Closing an already closed row means two paths ended the same session
    /// (for example an admin revoke racing an expiry sweep), which is a bug and not something to paper over.
    /// </summary>
    public void Close(LogoutReason reason, DateTime at)
    {
        if (IsClosed)
            throw new ConflictException(IamMessages.LoginLogAlreadyClosed);

        LoggedOutAt = at;
        LogoutReason = reason;
    }

    private static UserLoginLog Create(Guid? userId, Guid? sessionId, LoginResult result, LoginAttempt attempt) =>
        new()
        {
            UserId = userId,
            SessionId = sessionId,
            IdentifierAttempted = userId is null ? Mask(attempt.IdentifierAttempted) : TextRules.Truncate(attempt.IdentifierAttempted, 256) ?? string.Empty,
            Result = result,
            LoginAt = attempt.LoginAt,
            IpAddress = IPAddress.TryParse(attempt.Client.IpAddress, out var parsed) ? parsed : null,
            UserAgent = TextRules.Truncate(attempt.Client.UserAgent, 512),
            AcceptLanguage = TextRules.Truncate(attempt.AcceptLanguage, 128),
            DeviceType = DeviceType.Unknown,
            IsNewDevice = attempt.IsNewDevice,
            IsNewIp = attempt.IsNewIp,
            TraceId = TextRules.Truncate(attempt.TraceId, 64)
        };

    /// <summary>
    /// R22: an identifier that matched no account is stored masked, so the row records that the attempt happened
    /// without turning the log into a list of the names and addresses people typed. Masking happens here, in the
    /// aggregate, so no caller can forget it - a known user's identifier is kept as typed, because the account itself
    /// is already recorded in <see cref="UserId"/>.
    /// </summary>
    private static string Mask(string? value)
    {
        var text = TextRules.Truncate(value, 256);
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        var at = text.IndexOf('@');
        if (at > 0)
            return $"{text[..1]}***{text[at..]}";

        return text.Length <= 2 ? "**" : $"{text[..1]}***{text[^1..]}";
    }

    /// <summary>
    /// Fills the parsed user-agent fields (R22). Separate from creation because parsing is the caller's job (the
    /// <c>UAParser</c> port, D11) and must not make the aggregate depend on it.
    /// </summary>
    public void SetClientDetails(ClientDevice device)
    {
        BrowserName = TextRules.Truncate(device.BrowserName, 64);
        BrowserVersion = TextRules.Truncate(device.BrowserVersion, 32);
        OsName = TextRules.Truncate(device.OsName, 64);
        OsVersion = TextRules.Truncate(device.OsVersion, 32);
        DeviceType = device.DeviceType;
    }
}
