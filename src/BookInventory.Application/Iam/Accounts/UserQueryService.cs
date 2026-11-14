using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.BuildingBlocks.Domain.Models;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// R38/R39: the read side of the user list and the user detail page - the list with its search, filters, sort and
/// paging, the detail, the status timeline and the active sessions.
/// <para>
/// Every read goes through <see cref="IUserDirectory"/>, so the hidden system account is in none of them and an unknown
/// id and the system account id are answered with the same 404 (R43). The single exception is the actor lookup of the
/// timeline, which has to recognise that account in order to <em>hide</em> it.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserQueryService(
    IUserDirectory directory,
    IAccountStore accounts,
    IAuthenticationStore authStore)
{
    /// <summary>R38/R43: the paged list, filtered by role / status / lockout, searched over the four identity fields.</summary>
    public Task<IPagedList<UserSummary>> SearchAsync(UserSearchQuery query, CancellationToken cancellationToken) =>
        directory.SearchAsync(query, cancellationToken);

    /// <summary>R39/R43: the detail of one user.</summary>
    public Task<UserDetail> GetAsync(Guid id, CancellationToken cancellationToken) =>
        RequireVisibleAsync(id, cancellationToken);

    /// <summary>R33/R39: the append-only status timeline, newest first, with the actor of every change.</summary>
    public async Task<UserStatusHistoryItem[]> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        await RequireVisibleAsync(id, cancellationToken);

        var rows = await accounts.FindStatusHistoryAsync(id, cancellationToken);
        var actors = await LoadActorsAsync(rows.Select(row => row.ChangedBy), cancellationToken);

        return [.. rows.Select(row => ToHistoryItem(row, actors))];
    }

    /// <summary>R39: the sessions of one user that are still usable; the sessions tab revokes them from here.</summary>
    public async Task<UserSessionItem[]> GetSessionsAsync(Guid id, CancellationToken cancellationToken)
    {
        await RequireVisibleAsync(id, cancellationToken);

        var sessions = await authStore.FindActiveSessionsAsync(id, cancellationToken);
        return [.. sessions
            .OrderByDescending(session => session.LastSeenAt)
            .Select(session => new UserSessionItem(
                session.Id, session.CreatedAt, session.LastSeenAt, session.ExpiresAt, session.IpAddress, session.UserAgent))];
    }

    /// <summary>R43: the one place a user is turned into a 404, so an unknown id and the system account id cannot differ.</summary>
    private async Task<UserDetail> RequireVisibleAsync(Guid id, CancellationToken cancellationToken) =>
        await directory.GetVisibleByIdAsync(id, cancellationToken)
        ?? throw new NotFoundException(IamMessages.UserNotFound);

    /// <summary>R48: the accounts a timeline names, so a row can show who acted.</summary>
    private async Task<Dictionary<Guid, AppUser>> LoadActorsAsync(IEnumerable<Guid?> actorIds, CancellationToken cancellationToken)
    {
        var wanted = actorIds.Where(actorId => actorId is not null).Select(actorId => actorId!.Value).Distinct().ToArray();
        var actors = await accounts.FindActorsAsync(wanted, cancellationToken);
        return actors.ToDictionary(actor => actor.Id);
    }

    /// <summary>
    /// R32/R48: a row without an actor is the automatic lift of an expired suspension, and an action of the hidden
    /// system account is reported with neither its name nor its id - both come back nameless, so the screen shows one
    /// fixed label for them without the API having to choose the wording.
    /// </summary>
    private static UserStatusHistoryItem ToHistoryItem(UserStatusHistory row, Dictionary<Guid, AppUser> actors)
    {
        var actor = row.ChangedBy is { } actorId ? actors.GetValueOrDefault(actorId) : null;
        var isUnnamed = actor is null || actor.IsSystemAccount;

        return new UserStatusHistoryItem(
            row.Id, row.FromStatus, row.ToStatus, row.Reason, row.Until,
            isUnnamed ? null : row.ChangedBy, isUnnamed ? null : actor!.FullName, row.ChangedAt);
    }
}
