using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// R33: the one place a status change is written to the timeline. It is a separate use case from the one that changes the
/// status because the change has to reach the timeline from three places - the five Super Admin transitions, the
/// automatic lift of an expired suspension (R32) and, later, the agent onboarding workflow - and R33 asks for one row
/// per change, not one per caller.
/// <para>
/// The actor and the instant are read back from the aggregate (<c>status_changed_by</c> / <c>status_changed_at</c>)
/// instead of being passed in, so a history row can never disagree with the account it belongs to: the lift has no
/// caller and is recorded with <c>SYSTEM</c> exactly like the account is.
/// </para>
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class UserStatusHistoryWriter(IAccountStore store, TimeProvider timeProvider)
{
    /// <summary>Appends the row for a change the aggregate has already made. Nothing is returned and nothing is saved.</summary>
    public Task RecordAsync(AppUser user, StatusTransition transition, CancellationToken cancellationToken)
    {
        var history = UserStatusHistory.Record(
            user.Id,
            transition,
            user.StatusChangedBy ?? Guid.Empty,
            user.StatusChangedAt ?? timeProvider.GetUtcNow().UtcDateTime);

        return store.AddAsync(history, cancellationToken);
    }
}
