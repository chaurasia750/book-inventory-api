using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// R15: one active session of the caller as the "my sessions" screen shows it. The browser, OS and device come from the
/// login-log row that opened the session (R22), and <c>IsCurrent</c> marks the session that made this very request, so
/// the screen can offer "sign out" on the others and "sign out everywhere" for all of them.
/// </summary>
public sealed record OwnSessionItem(
    Guid Id,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    DateTime ExpiresAt,
    string? IpAddress,
    string? BrowserName,
    string? OsName,
    DeviceType? DeviceType,
    bool IsCurrent);

/// <summary>R26: one row of a user's own login history - what happened, from where, and how it ended.</summary>
public sealed record LoginHistoryItem(
    Guid Id,
    DateTime LoginAt,
    LoginResult Result,
    string? IpAddress,
    string? BrowserName,
    string? BrowserVersion,
    string? OsName,
    string? OsVersion,
    DeviceType DeviceType,
    DateTime? LoggedOutAt,
    LogoutReason? LogoutReason,
    bool IsNewDevice,
    bool IsNewIp);

/// <summary>
/// R48: who an audit or login-log row is about. The hidden system account is reported as <c>IsSystem</c> with neither its
/// user name nor its id, so the screen shows one fixed label and accountability exists without exposing the account.
/// </summary>
public sealed record ActorLabel(Guid? UserId, string? UserName, bool IsSystem);

/// <summary>R26: one row of the Super Admin's login-log search - <see cref="LoginHistoryItem"/> plus who and the raw user agent.</summary>
public sealed record LoginLogItem(
    Guid Id,
    ActorLabel Actor,
    string IdentifierAttempted,
    LoginResult Result,
    DateTime LoginAt,
    string? IpAddress,
    string? UserAgent,
    string? BrowserName,
    string? BrowserVersion,
    string? OsName,
    string? OsVersion,
    DeviceType DeviceType,
    DateTime? LoggedOutAt,
    LogoutReason? LogoutReason,
    bool IsNewDevice,
    bool IsNewIp);

/// <summary>A login-log row as the store reads it, before the actor label of R48 is decided.</summary>
public sealed record LoginLogRow(
    Guid Id,
    Guid? UserId,
    string? UserName,
    bool IsSystemAccount,
    string IdentifierAttempted,
    LoginResult Result,
    DateTime LoginAt,
    string? IpAddress,
    string? UserAgent,
    string? BrowserName,
    string? BrowserVersion,
    string? OsName,
    string? OsVersion,
    DeviceType DeviceType,
    DateTime? LoggedOutAt,
    LogoutReason? LogoutReason,
    bool IsNewDevice,
    bool IsNewIp);

/// <summary>R26: the filters of the Super Admin's login-log search and export.</summary>
public sealed record LoginLogQuery
{
    public Guid? UserId { get; init; }

    public Role? Role { get; init; }

    public LoginResult? Result { get; init; }

    /// <summary>An exact client address (IPv4 or IPv6); anything that is not an address is a 400.</summary>
    public string? IpAddress { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    /// <summary>One of <c>LoginAt</c> or <c>Result</c>, optionally followed by <c>ASC</c> or <c>DESC</c>.</summary>
    public string? OrderBy { get; init; } = "LoginAt DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}

/// <summary>
/// R41/R35: the caller's own profile. The e-mail and the mobile are optional (an omitted one keeps the stored value), and
/// changing either of them needs <c>CurrentPassword</c>, which is checked by the use case.
/// </summary>
public sealed record UpdateOwnProfileRequest(
    [param: Required, StringLength(IdentityNormalizer.FullNameMaxLength, MinimumLength = 1)] string FullName,
    [param: StringLength(IdentityNormalizer.EmailMaxLength, MinimumLength = 3)] string? Email,
    [param: StringLength(IdentityNormalizer.MobileMaxLength, MinimumLength = IdentityNormalizer.MobileMinLength)] string? Mobile,
    [param: StringLength(256)] string? CurrentPassword);
