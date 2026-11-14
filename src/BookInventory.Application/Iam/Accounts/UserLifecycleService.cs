using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// The five status transitions and the role change of R28/R30/R6, as use cases for <c>users/{id}/...</c>.
/// <para>
/// Each method is deliberately the same three steps: load the tracked account, call the one aggregate behaviour that
/// decides whether the change is allowed, and write the history row of R33. Whether a transition is legal, whether a
/// reason is long enough, whether the account is the last Super Admin and which sessions have to die are all answers of
/// the aggregate; this class never repeats them.
/// </para>
/// <para>
/// The session revocations are made here and not from the domain event, because a handler runs after the request
/// transaction has already saved (see <c>UserSessionRevoker</c>). The events of R40 are still raised by the aggregate,
/// for the modules that react to them.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserLifecycleService(
    IAccountStore store,
    IUserDirectory directory,
    UserStatusHistoryWriter history,
    UserSessionRevoker sessions,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
{
    /// <summary>
    /// R28/R57: approves a pending agent application. ACTIVE at once, first login must change the generated password,
    /// and the note is optional.
    /// </summary>
    public async Task ApproveAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Status;

        user.Approve(currentUser.UserId, Now(), request.Reason);
        await RecordAsync(user, from, cancellationToken);
    }

    /// <summary>R28: brings an inactive or suspended account back to ACTIVE. A pending account is approved, not activated.</summary>
    public async Task ActivateAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Status;

        user.Activate(currentUser.UserId, Now(), request.Reason);
        await RecordAsync(user, from, cancellationToken);
    }

    /// <summary>
    /// R28/R37/R31: retires an account. The reason is mandatory, every session of the user dies in the same transaction
    /// and the last visible Super Admin is refused.
    /// </summary>
    public async Task DeactivateAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Status;

        user.Deactivate(
            request.Reason ?? string.Empty,
            currentUser.UserId,
            Now(),
            await directory.LockAndCountOtherActiveVisibleSuperAdminsAsync(id, cancellationToken));

        await RecordAsync(user, from, cancellationToken);
        await sessions.RevokeAllAsync(user.Id, LogoutReason.AccountDeactivated, cancellationToken);
    }

    /// <summary>
    /// R30/R31: suspends an account with a mandatory reason and an optional end date in the future. Sessions die now; when
    /// the end date passes the account lifts to ACTIVE by itself (R32, the lift job).
    /// </summary>
    public async Task SuspendAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Status;

        user.Suspend(
            new SuspensionRequest(request.Reason ?? string.Empty, request.SuspendedUntil),
            currentUser.UserId,
            Now(),
            await directory.LockAndCountOtherActiveVisibleSuperAdminsAsync(id, cancellationToken));

        await RecordAsync(user, from, cancellationToken);
        await sessions.RevokeAllAsync(user.Id, LogoutReason.AccountSuspended, cancellationToken);
    }

    /// <summary>
    /// R28/R37/R58: refuses a pending agent application. The account stays as history and keeps its e-mail and mobile
    /// reserved until a Super Admin releases them.
    /// </summary>
    public async Task RejectAsync(Guid id, StatusChangeRequest request, CancellationToken cancellationToken)
    {
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Status;

        user.Reject(request.Reason ?? string.Empty, currentUser.UserId, Now());
        await RecordAsync(user, from, cancellationToken);
    }

    /// <summary>
    /// R6/AC24: moves a user between the staff roles with a mandatory reason. Never the caller's own account, never to or
    /// from MEMBER, never the last Super Admin. The role timeline of AC24 is written here and the sessions are revoked
    /// because the permissions behind the token have changed.
    /// </summary>
    public async Task ChangeRoleAsync(Guid id, ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var role = request.Role ?? throw new ValidationFailureException(IamMessages.UserRoleRequired);
        var user = await LoadAsync(id, cancellationToken);
        var from = user.Role;
        var now = Now();

        user.ChangeRole(
            new RoleChange(role, request.Reason ?? string.Empty),
            currentUser.UserId,
            now,
            await directory.LockAndCountOtherActiveVisibleSuperAdminsAsync(id, cancellationToken));

        await store.AddAsync(
            UserRoleHistory.Record(user.Id, new RoleTransition(from, user.Role, request.Reason), currentUser.UserId, now),
            cancellationToken);
        await sessions.RevokeAllAsync(user.Id, LogoutReason.RevokedByAdmin, cancellationToken);
    }

    /// <summary>404 when no such id at all; the hidden system account is loaded on purpose and refuses with 403 (R42).</summary>
    private async Task<AppUser> LoadAsync(Guid id, CancellationToken cancellationToken) =>
        await store.FindForManagementAsync(id, cancellationToken)
        ?? throw new NotFoundException(IamMessages.UserNotFound);

    /// <summary>
    /// R33: one row per change, written from the state the aggregate actually ended up in - the reason it accepted, the
    /// end date it stored and the status it moved to - so the timeline can never drift from the account.
    /// </summary>
    private async Task RecordAsync(AppUser user, AccountStatus from, CancellationToken cancellationToken) =>
        await history.RecordAsync(
            user, new StatusTransition(from, user.Status, user.StatusReason, user.SuspendedUntil), cancellationToken);

    private DateTime Now() => timeProvider.GetUtcNow().UtcDateTime;
}
