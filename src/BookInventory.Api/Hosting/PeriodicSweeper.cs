namespace BookInventory.Api.Hosting;

/// <summary>
/// The shape every periodic clean-up job of the host shares: wake up every <see cref="Interval"/>, do the work in a scope
/// of its own, log what was handled, and treat a failure as a warning - the next tick tries again and nothing about the
/// requests being served depends on the job. Only the work itself differs between jobs, so it is the one thing a
/// subclass writes (<see cref="SweepAsync"/>); the timer, the scope and the error handling exist once.
/// </summary>
public abstract class PeriodicSweeper(IServiceScopeFactory scopeFactory, ILogger logger) : BackgroundService
{
    /// <summary>How often the job runs.</summary>
    protected abstract TimeSpan Interval { get; }

    /// <summary>What the job is called in the log ("expired-session sweep").</summary>
    protected abstract string Name { get; }

    /// <summary>Does one pass of the work and returns how many items it handled (0 is quiet).</summary>
    protected abstract Task<int> SweepAsync(IServiceProvider services, CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            await TickAsync(stoppingToken);
    }

    private async Task TickAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var handled = await SweepAsync(scope.ServiceProvider, cancellationToken);
            if (handled > 0)
                logger.LogInformation("The {Job} handled {Count} item(s).", Name, handled);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // shutting down
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "The {Job} failed and will run again on the next tick.", Name);
        }
    }
}
