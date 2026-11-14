using System.Security.Cryptography;
using System.Text;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// One logged-in device of one user (R11). It is its own aggregate root because it is reached by the refresh token it
/// holds, not through the user, and because sessions of different users are revoked independently.
/// <list type="bullet">
/// <item>Only the hash of the refresh token is stored; the token itself lives in an HttpOnly cookie (D2).</item>
/// <item><see cref="Rotate"/> implements the reuse detection of R13: a token that no longer matches the stored hash was
/// already used, so the session is revoked with <see cref="LogoutReason.RefreshReuse"/> instead of being extended.</item>
/// <item><see cref="Revoke"/> is one-way. The session duration is derived from <see cref="ExpiresAt"/>, never stored
/// separately (R24).</item>
/// </list>
/// </summary>
public class UserSession : AggregateRoot
{
    private UserSession() => RefreshTokenHash = string.Empty;

    public Guid UserId { get; private set; }

    /// <summary>SHA-256 (or stronger) of the current refresh token. Unique across all sessions (R11).</summary>
    public string RefreshTokenHash { get; private set; }

    /// <summary>
    /// The hash of the token this one replaced (R13). It exists only so a replayed token can still be attributed to
    /// this session and revoke it; it is never accepted, and one generation back is enough - a token older than that
    /// matches nothing and is answered 401 like any unknown token.
    /// </summary>
    public string? PreviousRefreshTokenHash { get; private set; }

    /// <summary>R11: this session was opened with "remember me", so rotations extend it by 30 days, not by 12 hours.</summary>
    public bool IsRememberMe { get; private set; }

    /// <summary>When this session stops being usable. The refresh cookie is scoped to the auth path and rotates within it.</summary>
    public DateTime ExpiresAt { get; private set; }

    public DateTime? LastSeenAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public LogoutReason? RevokeReason { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    /// <summary>R69: the company a Super Admin entered in this session, or null. Set and cleared only through the capability of the <c>company</c> module.</summary>
    public Guid? ActiveCompanyId { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public bool IsActiveAt(DateTime utcNow) => !IsRevoked && ExpiresAt > utcNow;

    /// <summary>Whether this session has run past its lifetime without anyone revoking it (R24: closed as <c>EXPIRED</c>).</summary>
    public bool IsExpiredAt(DateTime utcNow) => !IsRevoked && ExpiresAt <= utcNow;

    public static UserSession Open(Guid userId, SessionGrant grant, ClientInfo client, DateTime utcNow) =>
        new()
        {
            UserId = userId,
            RefreshTokenHash = TextRules.RequireHash(grant.RefreshTokenHash, IamMessages.SessionNotFound),
            IsRememberMe = grant.RememberMe,
            ExpiresAt = grant.ExpiresAt,
            LastSeenAt = utcNow,
            IpAddress = TextRules.Truncate(client.IpAddress, 64),
            UserAgent = TextRules.Truncate(client.UserAgent, 512)
        };

    /// <summary>
    /// R11/R13: rotates the refresh token and returns <c>true</c>. A presented token that does not match the stored
    /// hash was already used: the whole session is revoked (<see cref="LogoutReason.RefreshReuse"/>) and <c>false</c> is
    /// returned. It does not throw on purpose - an exception rolls the request back and the revoke would be lost - so
    /// the caller commits (<c>[PersistOnFailure]</c>) and then answers <see cref="IamMessages.SessionRefreshReuse"/>.
    /// </summary>
    public bool Rotate(string presentedTokenHash, string newTokenHash, DateTime newExpiresAt, DateTime utcNow)
    {
        EnsureActiveAt(utcNow);

        if (!FixedTimeEquals(RefreshTokenHash, TextRules.RequireHash(presentedTokenHash, IamMessages.SessionNotFound)))
        {
            Revoke(LogoutReason.RefreshReuse, utcNow);
            return false;
        }

        PreviousRefreshTokenHash = RefreshTokenHash;
        RefreshTokenHash = TextRules.RequireHash(newTokenHash, IamMessages.SessionNotFound);
        ExpiresAt = newExpiresAt;
        LastSeenAt = utcNow;
        return true;
    }

    /// <summary>R14/R18/R19/R31/R36: ends the session. Idempotent, so a second revoke is not an error.</summary>
    public void Revoke(LogoutReason reason, DateTime utcNow)
    {
        if (IsRevoked)
            return;

        RevokedAt = utcNow;
        RevokeReason = reason;
    }

    /// <summary>R69: a Super Admin session starts working for a company. The caller (the capability) has checked the role.</summary>
    public void EnterCompany(Guid companyId) =>
        ActiveCompanyId = companyId != Guid.Empty ? companyId : throw new ValidationFailureException(IamMessages.UserCompanyInvalid);

    /// <summary>R69: back to no company.</summary>
    public void LeaveCompany() => ActiveCompanyId = null;

    private void EnsureActiveAt(DateTime utcNow)
    {
        if (IsRevoked)
            throw new UnauthenticatedException(IamMessages.SessionRevoked);
        if (ExpiresAt <= utcNow)
            throw new UnauthenticatedException(IamMessages.SessionExpired);
    }

    /// <summary>Token hashes are compared without leaking their content through timing.</summary>
    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
}
