using System.Net;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.History;

/// <summary>
/// R26: who sees which login rows. <see cref="OwnAsync"/> is every user's own history - the id is the caller's, from the
/// token - and <see cref="SearchAsync"/> / <see cref="ExportCsvAsync"/> are the Super Admin's whole-log view, guarded by the
/// role on the controller. Rows of the hidden system account are relabelled through <see cref="IActorLabelResolver"/> (R48).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class LoginHistoryService(
    IHistoryStore history,
    IActorLabelResolver actors,
    ICurrentUser currentUser)
{
    /// <summary>How many rows one export may hold; a bigger match is refused with a message to narrow the filters.</summary>
    public const int ExportRowCap = 10_000;

    public Task<IPagedList<LoginHistoryItem>> OwnAsync(int pageIndex, int pageSize, CancellationToken cancellationToken) =>
        history.FindOwnLoginHistoryAsync(currentUser.UserId, Math.Max(1, pageIndex), Math.Clamp(pageSize, 1, 100), cancellationToken);

    public async Task<PagedResult<LoginLogItem>> SearchAsync(LoginLogQuery query, CancellationToken cancellationToken)
    {
        EnsureValid(query);
        var page = await history.SearchLoginLogsAsync(query, cancellationToken);

        return new PagedResult<LoginLogItem>(
            [.. page.Items.Select(ToItem)], page.PageIndex, page.PageSize, page.TotalCount, page.TotalPages);
    }

    /// <summary>The CSV of the same filters, or a 400 when more than <see cref="ExportRowCap"/> rows match.</summary>
    public async Task<string> ExportCsvAsync(LoginLogQuery query, CancellationToken cancellationToken)
    {
        EnsureValid(query);
        if (await history.CountLoginLogsAsync(query, cancellationToken) > ExportRowCap)
            throw new ValidationFailureException(IamMessages.LoginLogExportTooLarge);

        var rows = await history.ExportLoginLogsAsync(query, ExportRowCap, cancellationToken);
        return LoginLogCsv.Build(rows.Select(ToItem));
    }

    private LoginLogItem ToItem(LoginLogRow row) => new(
        row.Id, actors.Resolve(row), row.IdentifierAttempted, row.Result, row.LoginAt, row.IpAddress, row.UserAgent,
        row.BrowserName, row.BrowserVersion, row.OsName, row.OsVersion, row.DeviceType,
        row.LoggedOutAt, row.LogoutReason, row.IsNewDevice, row.IsNewIp);

    private static void EnsureValid(LoginLogQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.IpAddress) && !IPAddress.TryParse(query.IpAddress.Trim(), out _))
            throw new ValidationFailureException(IamMessages.LoginLogInvalidIp);
    }
}

/// <summary>A page of already projected rows, in the shape the API returns (the same fields as the platform's paged list).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int PageIndex, int PageSize, int TotalCount, int TotalPages);
