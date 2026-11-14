using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Domain.Iam;

namespace BookInventory.Api.Hosting;

/// <summary>
/// R32: a suspension whose <c>suspended_until</c> has passed lifts to <c>ACTIVE</c> by itself, recorded with actor
/// <c>SYSTEM</c> and with a row in the status timeline (R33), so the user list is right again without anybody touching it.
/// Login lifts such a suspension lazily as well (Plan 0004), and that is the part that matters for security: this job
/// exists so the <em>screens</em> do not keep showing a stale SUSPENDED.
/// </summary>
public sealed class SuspensionLiftSweeper(IServiceScopeFactory scopeFactory, ILogger<SuspensionLiftSweeper> logger)
    : PeriodicSweeper(scopeFactory, logger)
{
    private const int BatchSize = 200;

    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override string Name => "suspension-lift sweep";

    protected override async Task<int> SweepAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var store = services.GetRequiredService<IAccountStore>();
        var history = services.GetRequiredService<UserStatusHistoryWriter>();
        var now = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

        var lifted = 0;
        foreach (var user in await store.FindDueSuspensionsAsync(now, BatchSize, cancellationToken))
        {
            // Read the end date before the aggregate clears it, so the timeline says when the suspension was due to end.
            var until = user.SuspendedUntil;
            if (!user.LiftSuspensionIfDue(now))
                continue;

            await history.RecordAsync(
                user, new StatusTransition(AccountStatus.Suspended, user.Status, user.StatusReason, until), cancellationToken);
            lifted++;
        }

        if (lifted > 0)
            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);

        return lifted;
    }
}
