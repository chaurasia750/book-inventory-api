using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// The claims of an access token (R11). A port, so no use case depends on a JWT library: the Infrastructure
/// implementation signs them and nothing else knows how.
/// </summary>
public sealed record AccessTokenRequest(
    Guid UserId,
    string UserName,
    Role Role,
    Guid SessionId,
    bool MustChangePassword,
    TimeSpan Lifetime,
    bool IsSystemAccount = false,
    Guid? CompanyId = null);

/// <summary>A signed access token and when it stops being valid.</summary>
public sealed record IssuedAccessToken(string Token, DateTime ExpiresAt);

/// <summary>
/// R11/D2: the 256-bit random refresh token, its SHA-256 (the only form that is ever stored, in
/// <see cref="UserSession.RefreshTokenHash"/>) and when it stops being usable.
/// </summary>
public sealed record IssuedOpaqueToken(string Token, string TokenHash, DateTime ExpiresAt);

/// <summary>
/// The claim names of the access token (R11). They live here, next to the use cases, because both ends of the token
/// need them and they are API surface: the Infrastructure side writes them, the API side reads the forced-change one.
/// </summary>
public static class AuthClaims
{
    /// <summary>R12: tells the server - not only the UI - that the password must be changed before anything else.</summary>
    public const string MustChangePassword = "pwd_change";

    /// <summary>How the caller proved who they are; always <c>pwd</c>, because 2FA is out of scope (spec section 1).</summary>
    public const string AuthenticationMethod = "amr";

    /// <summary>R44: present (value <see cref="SystemAccountValue"/>) only on the hidden system account's token; used to label audit entries and by the <c>FinancialAction</c> policy (R49).</summary>
    public const string SystemAccount = "sys";

    public const string SystemAccountValue = "1";

    /// <summary>R62: the company of the caller (a company account's own, or the one a Super Admin entered). Absent when there is none.</summary>
    public const string CompanyId = "cid";

    /// <summary>The value of <see cref="AuthenticationMethod"/> for a password login.</summary>
    public const string Password = "pwd";
}

/// <summary>The last successful client of a user, used to answer "is this device/IP new?" (R22).</summary>
public sealed record PreviousClient(string? UserAgent, string? IpAddress);
