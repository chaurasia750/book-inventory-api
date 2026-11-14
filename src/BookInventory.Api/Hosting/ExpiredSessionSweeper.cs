using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Application.Iam.Authentication;
using BookInventory.Domain.Iam;

namespace BookInventory.Api.Hosting;

/// <summary>
/// R24: a session that simply runs out is closed with <c>EXPIRED</c> and <c>logged_out_at = expires_at</c>. The refresh
/// use case already closes an expired session lazily (R24 allows both), but a user who never comes back would leave a
/// row saying "still logged in" forever and a session the sessions screen keeps listing. It touches only sessions that
/// really ran out, at most <see cref="BatchSize"/> per tick, and ends them through <see cref="SessionEndService"/>, the
/// same path logout and refresh use.
/// </summary>
public sealed class ExpiredSessionSweeper(IServiceScopeFactory scopeFactory, ILogger<ExpiredSessionSweeper> logger)
    : PeriodicSweeper(scopeFactory, logger)
{
    private const int BatchSize = 500;

    protected override TimeSpan Interval => TimeSpan.FromMinutes(5);

    protected override string Name => "expired-session sweep";

    protected override async Task<int> SweepAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var store = services.GetRequiredService<IAuthenticationStore>();
        var sessionEnd = services.GetRequiredService<SessionEndService>();
        var now = services.GetRequiredService<TimeProvider>().GetUtcNow().UtcDateTime;

        var expired = await store.FindExpiredSessionsAsync(now, BatchSize, cancellationToken);
        foreach (var session in expired)
            await sessionEnd.EndAsync(session, LogoutReason.Expired, session.ExpiresAt, cancellationToken);

        if (expired.Length > 0)
            await services.GetRequiredService<IUnitOfWork>().SaveChangesAsync(cancellationToken);

        return expired.Length;
    }
}
