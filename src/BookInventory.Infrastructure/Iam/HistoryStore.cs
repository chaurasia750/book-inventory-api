using System.Net;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Iam.History;
using BookInventory.Domain.Iam;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// The read side of the login history and the login-log search (Plan 0007): projections only, no tracking, and the paging
/// and sorting are written here on purpose - the sort accepts <c>LoginAt</c> and <c>Result</c> and nothing else, so a query
/// string cannot order by (and so probe) any other column. The hidden system account is joined in for one reason: to be
/// recognised and relabelled by the caller (R48), never to be shown as it is.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IHistoryStore))]
public sealed class HistoryStore : IHistoryStore
{
    private const int MaxPageSize = 200;

    private readonly IRepository<UserLoginLog> _logs;
    private readonly IRepository<UserSession> _sessions;
    private readonly IRepository<AppUser> _users;

    public HistoryStore(IUnitOfWork unitOfWork)
    {
        _logs = unitOfWork.Repository<UserLoginLog>();
        _sessions = unitOfWork.Repository<UserSession>();
        _users = unitOfWork.Repository<AppUser>();
    }

    public async Task<OwnSessionItem[]> FindOwnSessionsAsync(
        Guid userId, Guid? currentSessionId, DateTime now, CancellationToken cancellationToken = default)
    {
        var rows = await (
                from session in _sessions.TableNoTracking
                where session.UserId == userId && session.RevokedAt == null && session.ExpiresAt > now
                join log in _logs.TableNoTracking.Where(l => l.Result == LoginResult.Success) on session.Id equals log.SessionId into logs
                from log in logs.DefaultIfEmpty()
                orderby session.LastSeenAt descending
                select new { session, log })
            .ToArrayAsync(cancellationToken);

        return [.. rows.Select(row => new OwnSessionItem(
            row.session.Id, row.session.CreatedAt, row.session.LastSeenAt, row.session.ExpiresAt, row.session.IpAddress,
            row.log?.BrowserName, row.log?.OsName, row.log?.DeviceType, row.session.Id == currentSessionId))];
    }

    public async Task<IPagedList<LoginHistoryItem>> FindOwnLoginHistoryAsync(
        Guid userId, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
    {
        var source = _logs.TableNoTracking.Where(l => l.UserId == userId).OrderByDescending(l => l.LoginAt);
        var (rows, total) = await PageAsync(source, pageIndex, pageSize, cancellationToken);

        return ToPage(
            rows.Select(l => new LoginHistoryItem(
                l.Id, l.LoginAt, l.Result, l.IpAddress?.ToString(), l.BrowserName, l.BrowserVersion, l.OsName, l.OsVersion,
                l.DeviceType, l.LoggedOutAt, l.LogoutReason, l.IsNewDevice, l.IsNewIp)),
            pageIndex, pageSize, total);
    }

    public async Task<IPagedList<LoginLogRow>> SearchLoginLogsAsync(LoginLogQuery query, CancellationToken cancellationToken = default)
    {
        var (rows, total) = await PageAsync(Filtered(query), query.PageIndex, query.PageSize, cancellationToken);
        return ToPage(rows.Select(ToRow), query.PageIndex, query.PageSize, total);
    }

    public Task<int> CountLoginLogsAsync(LoginLogQuery query, CancellationToken cancellationToken = default) =>
        Filtered(query).CountAsync(cancellationToken);

    public async Task<LoginLogRow[]> ExportLoginLogsAsync(LoginLogQuery query, int take, CancellationToken cancellationToken = default) =>
        [.. (await Filtered(query).Take(take).ToArrayAsync(cancellationToken)).Select(ToRow)];

    /// <summary>
    /// The filters of R26 and the whitelisted sort, over the log joined to its user (if any). It projects <em>columns</em>, not
    /// entities: an entity that is null on the left side of a join cannot be constructed in SQL, and only these columns are needed.
    /// </summary>
    private IQueryable<LogWithUser> Filtered(LoginLogQuery query)
    {
        var users = _users.TableNoTracking.IgnoreHiddenAccountFilter();
        var source =
            from log in _logs.TableNoTracking
            join user in users on log.UserId equals user.Id into joined
            from user in joined.DefaultIfEmpty()
            select new LogWithUser
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = user == null ? null : user.UserName,
                UserRole = user == null ? null : user.Role,
                IsSystemAccount = user != null && user.IsSystemAccount,
                IdentifierAttempted = log.IdentifierAttempted,
                Result = log.Result,
                LoginAt = log.LoginAt,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                BrowserName = log.BrowserName,
                BrowserVersion = log.BrowserVersion,
                OsName = log.OsName,
                OsVersion = log.OsVersion,
                DeviceType = log.DeviceType,
                LoggedOutAt = log.LoggedOutAt,
                LogoutReason = log.LogoutReason,
                IsNewDevice = log.IsNewDevice,
                IsNewIp = log.IsNewIp
            };

        if (query.UserId is { } userId)
            source = source.Where(x => x.UserId == userId);
        if (query.Role is { } role)
            source = source.Where(x => x.UserRole == role);
        if (query.Result is { } result)
            source = source.Where(x => x.Result == result);
        if (!string.IsNullOrWhiteSpace(query.IpAddress) && IPAddress.TryParse(query.IpAddress.Trim(), out var ip))
            source = source.Where(x => x.IpAddress == ip);
        if (query.From is { } from)
            source = source.Where(x => x.LoginAt >= from);
        if (query.To is { } to)
            source = source.Where(x => x.LoginAt <= to);

        return Sorted(source, query.OrderBy);
    }

    /// <summary><c>LoginAt</c> or <c>Result</c>, ascending or descending; anything else falls back to newest first.</summary>
    private static IQueryable<LogWithUser> Sorted(IQueryable<LogWithUser> source, string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var descending = parts.Length < 2 || !parts[1].Equals("ASC", StringComparison.OrdinalIgnoreCase);

        return parts.Length > 0 && parts[0].Equals("Result", StringComparison.OrdinalIgnoreCase)
            ? descending ? source.OrderByDescending(x => x.Result).ThenByDescending(x => x.LoginAt) : source.OrderBy(x => x.Result).ThenByDescending(x => x.LoginAt)
            : descending ? source.OrderByDescending(x => x.LoginAt) : source.OrderBy(x => x.LoginAt);
    }

    private static async Task<(T[] Rows, int Total)> PageAsync<T>(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken)
    {
        var size = Math.Clamp(pageSize, 1, MaxPageSize);
        var index = Math.Max(1, pageIndex);
        var total = await source.CountAsync(cancellationToken);
        var rows = await source.Skip((index - 1) * size).Take(size).ToArrayAsync(cancellationToken);
        return (rows, total);
    }

    private static PagedList<T> ToPage<T>(IEnumerable<T> items, int pageIndex, int pageSize, int total)
    {
        var size = Math.Clamp(pageSize, 1, MaxPageSize);
        return new PagedList<T>
        {
            Items = [.. items],
            PageIndex = Math.Max(1, pageIndex),
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
            IndexFrom = 1
        };
    }

    private static LoginLogRow ToRow(LogWithUser x) => new(
        x.Id, x.UserId, x.UserName, x.IsSystemAccount, x.IdentifierAttempted, x.Result, x.LoginAt, x.IpAddress?.ToString(),
        x.UserAgent, x.BrowserName, x.BrowserVersion, x.OsName, x.OsVersion, x.DeviceType, x.LoggedOutAt, x.LogoutReason,
        x.IsNewDevice, x.IsNewIp);

    /// <summary>The columns of a login-log row and of its user that the search needs, read in one query.</summary>
    private sealed class LogWithUser
    {
        public Guid Id { get; init; }
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public Role? UserRole { get; init; }
        public bool IsSystemAccount { get; init; }
        public string IdentifierAttempted { get; init; } = string.Empty;
        public LoginResult Result { get; init; }
        public DateTime LoginAt { get; init; }
        public IPAddress? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public string? BrowserName { get; init; }
        public string? BrowserVersion { get; init; }
        public string? OsName { get; init; }
        public string? OsVersion { get; init; }
        public DeviceType DeviceType { get; init; }
        public DateTime? LoggedOutAt { get; init; }
        public LogoutReason? LogoutReason { get; init; }
        public bool IsNewDevice { get; init; }
        public bool IsNewIp { get; init; }
    }
}
