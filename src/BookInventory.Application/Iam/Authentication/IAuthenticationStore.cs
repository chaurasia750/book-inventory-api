using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Authentication;

/// <summary>
/// The data the authentication use cases need, as one persistence port. It is not <c>IRepository&lt;T&gt;</c>,
/// because the login lookup and <c>/me</c> are the only two places in the system allowed to see past the hidden
/// system-account filter (R43, R45) and that bypass must live in the data layer, not in a use case.
/// <para>
/// The port deliberately has no save method: a write request is committed by the request transaction
/// (<c>TransactionFilter</c>, and <c>[PersistOnFailure]</c> for the ones that must survive a 4xx), so a use case cannot
/// half-save. A caller outside HTTP (a hosted job) uses <c>IUnitOfWork.SaveChangesAsync</c> itself.
/// </para>
/// </summary>
public interface IAuthenticationStore
{
    /// <summary>
    /// R7/R45: the account behind a login identifier - a user name for staff and agents, an e-mail for staff only.
    /// Mobile is never a login identifier, and an agent cannot be found by e-mail. Includes the hidden system
    /// account, which logs in through this same path.
    /// </summary>
    Task<AppUser?> FindUserForLoginAsync(string identifier, CancellationToken cancellationToken = default);

    /// <summary>
    /// R44: the caller's own account for <c>GET auth/me</c>, found by the id in the token. Includes the hidden
    /// system account, which is why the filter is bypassed here; it is never used for a list, a search or another
    /// user's id (that is <c>IUserDirectory</c>).
    /// </summary>
    Task<AppUser?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserSession session, CancellationToken cancellationToken = default);

    Task AddAsync(UserLoginLog loginLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// R11/R13: the session a presented refresh token belongs to - found by the current token hash or by the one it
    /// replaced, so a replayed token can still be attributed and its session revoked. Change-tracked for rotation.
    /// </summary>
    Task<UserSession?> FindSessionByPresentedTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<UserSession?> FindSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>R14: every session of one user, for logout-all.</summary>
    Task<UserSession[]> FindActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>R24: sessions that ran out without being revoked, oldest first, at most <paramref name="take"/> - for the periodic sweep.</summary>
    Task<UserSession[]> FindExpiredSessionsAsync(DateTime now, int take, CancellationToken cancellationToken = default);

    /// <summary>R31: whether a session exists, is not revoked and has not run out - a read-only check, nothing is tracked.</summary>
    Task<bool> IsSessionActiveAsync(Guid sessionId, DateTime now, CancellationToken cancellationToken = default);

    /// <summary>R24: the still-open login-log row of a session, so it can be closed exactly once.</summary>
    Task<UserLoginLog?> FindOpenLogBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default);

    /// <summary>R17: the account behind an e-mail address (normalised), or <c>null</c>. The hidden system account is never found.</summary>
    Task<AppUser?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default);

    Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);

    /// <summary>R17/R18: the reset token with this hash (change-tracked, so using it can mark it used).</summary>
    Task<PasswordResetToken?> FindResetTokenAsync(string tokenHash, CancellationToken cancellationToken = default);

    /// <summary>R17: the unused reset tokens of one user, which a new request invalidates.</summary>
    Task<PasswordResetToken[]> FindUnusedResetTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>R17: how many reset tokens one user asked for since a moment, for the per-hour limit.</summary>
    Task<int> CountResetTokensSinceAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default);

    /// <summary>R22: the last successful login of a user, to tell a new device or IP from a known one.</summary>
    Task<PreviousClient?> FindPreviousClientAsync(Guid userId, CancellationToken cancellationToken = default);
}
