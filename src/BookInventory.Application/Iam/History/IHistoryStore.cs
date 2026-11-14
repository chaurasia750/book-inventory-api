using BookInventory.BuildingBlocks.Domain.Models;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// The read side of the login history, the own-sessions screen and the Super Admin's login-log search (Plan 0007): only
/// projections, never an entity, and no tracking. It looks past the hidden-account filter for one reason - a log row of
/// the system account has to be recognised in order to be <em>relabelled</em> (R48), never to be shown as it is.
/// </summary>
public interface IHistoryStore
{
    /// <summary>R15: the caller's sessions that are still usable, newest first, each with the device of the login that opened it.</summary>
    Task<OwnSessionItem[]> FindOwnSessionsAsync(Guid userId, Guid? currentSessionId, DateTime now, CancellationToken cancellationToken = default);

    /// <summary>R26: the caller's own login rows, newest first.</summary>
    Task<IPagedList<LoginHistoryItem>> FindOwnLoginHistoryAsync(Guid userId, int pageIndex, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>R26: the filtered, sorted, paged login-log search of a Super Admin.</summary>
    Task<IPagedList<LoginLogRow>> SearchLoginLogsAsync(LoginLogQuery query, CancellationToken cancellationToken = default);

    /// <summary>R26: how many rows the same filters match, so an export that is too large can be refused before it is built.</summary>
    Task<int> CountLoginLogsAsync(LoginLogQuery query, CancellationToken cancellationToken = default);

    /// <summary>R26: the rows of an export - the same filters and sort, at most <paramref name="take"/> of them.</summary>
    Task<LoginLogRow[]> ExportLoginLogsAsync(LoginLogQuery query, int take, CancellationToken cancellationToken = default);
}
