using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// The data side of the authentication use cases. Two queries here look past the hidden system-account filter of R43
/// and only two: the login lookup (R45 - the system account logs in through the same flow as anybody else) and the
/// caller's own <c>/me</c> (R44). No list, search, count or export ever comes through this class.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IAuthenticationStore))]
public sealed class AuthenticationStore : IAuthenticationStore
{
    private readonly IRepository<AppUser> _users;
    private readonly IRepository<UserSession> _sessions;
    private readonly IRepository<UserLoginLog> _loginLogs;
    private readonly IRepository<PasswordResetToken> _resetTokens;

    public AuthenticationStore(IUnitOfWork unitOfWork)
    {
        _users = unitOfWork.Repository<AppUser>();
        _sessions = unitOfWork.Repository<UserSession>();
        _loginLogs = unitOfWork.Repository<UserLoginLog>();
        _resetTokens = unitOfWork.Repository<PasswordResetToken>();
    }

    /// <summary>
    /// R7/R64: the user name matches everybody, the e-mail only staff - an agent and a company account (Owner, Staff)
    /// must log in with their generated user id and cannot walk in through their e-mail (AC2, AC35). Mobile is not a
    /// login identifier at all.
    /// </summary>
    public Task<AppUser?> FindUserForLoginAsync(string identifier, CancellationToken cancellationToken = default)
    {
        var userName = IdentityNormalizer.NormalizeUserName(identifier) ?? string.Empty;
        var email = IdentityNormalizer.NormalizeEmail(identifier) ?? string.Empty;

        return _users.Table
            .IgnoreHiddenAccountFilter()
            .FirstOrDefaultAsync(
                u => u.UserName == userName
                     || (u.Email == email && u.Role != Role.Member && u.Role != Role.Owner && u.Role != Role.Staff),
                cancellationToken);
    }

    public Task<AppUser?> FindUserByIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _users.Table.FindByIdIncludingHiddenAsync(userId, cancellationToken);

    public Task<AppUser?> FindUserByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = default) =>
        _users.Table.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

    public Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default) =>
        _resetTokens.AddAsync(token, cancellationToken);

    public Task<PasswordResetToken?> FindResetTokenAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _resetTokens.Table.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public Task<PasswordResetToken[]> FindUnusedResetTokensAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _resetTokens.Table.Where(t => t.UserId == userId && t.UsedAt == null).ToArrayAsync(cancellationToken);

    public Task<int> CountResetTokensSinceAsync(Guid userId, DateTime since, CancellationToken cancellationToken = default) =>
        _resetTokens.TableNoTracking.CountAsync(t => t.UserId == userId && t.CreatedAt >= since, cancellationToken);

    public Task AddAsync(UserSession session, CancellationToken cancellationToken = default) =>
        _sessions.AddAsync(session, cancellationToken);

    public Task AddAsync(UserLoginLog loginLog, CancellationToken cancellationToken = default) =>
        _loginLogs.AddAsync(loginLog, cancellationToken);

    /// <summary>
    /// R13: a session is found by its current token hash <em>or</em> the one it replaced. That second match is what
    /// makes reuse detection possible: a replayed token is already useless, but it still has to be attributable to the
    /// session it belongs to so that session can be revoked.
    /// </summary>
    public Task<UserSession?> FindSessionByPresentedTokenAsync(string tokenHash, CancellationToken cancellationToken = default) =>
        _sessions.Table.FirstOrDefaultAsync(
            s => s.RefreshTokenHash == tokenHash || s.PreviousRefreshTokenHash == tokenHash,
            cancellationToken);

    public Task<UserSession?> FindSessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        _sessions.Table.FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

    public Task<UserSession[]> FindActiveSessionsAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _sessions.Table
            .Where(s => s.UserId == userId && s.RevokedAt == null)
            .ToArrayAsync(cancellationToken);

    public Task<UserSession[]> FindActiveSessionsOfCompanyAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _sessions.Table
            .Where(s => s.RevokedAt == null && _users.Table.Any(u => u.Id == s.UserId && u.CompanyId == companyId))
            .ToArrayAsync(cancellationToken);

    public Task<UserSession[]> FindExpiredSessionsAsync(DateTime now, int take, CancellationToken cancellationToken = default) =>
        _sessions.Table
            .Where(s => s.RevokedAt == null && s.ExpiresAt <= now)
            .OrderBy(s => s.ExpiresAt)
            .Take(take)
            .ToArrayAsync(cancellationToken);

    public Task<bool> IsSessionActiveAsync(Guid sessionId, DateTime now, CancellationToken cancellationToken = default) =>
        _sessions.TableNoTracking.AnyAsync(
            s => s.Id == sessionId && s.RevokedAt == null && s.ExpiresAt > now,
            cancellationToken);

    public Task<UserLoginLog?> FindOpenLogBySessionAsync(Guid sessionId, CancellationToken cancellationToken = default) =>
        _loginLogs.Table.FirstOrDefaultAsync(l => l.SessionId == sessionId && l.LoggedOutAt == null, cancellationToken);

    /// <summary>R22: the newest successful login of this user, to compare the device and the IP against.</summary>
    public async Task<PreviousClient?> FindPreviousClientAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var last = await _loginLogs.TableNoTracking
            .Where(l => l.UserId == userId && l.Result == LoginResult.Success)
            .OrderByDescending(l => l.LoginAt)
            .FirstOrDefaultAsync(cancellationToken);

        return last is null ? null : new PreviousClient(last.UserAgent, last.IpAddress?.ToString());
    }
}
