using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using BookInventory.Infrastructure.Persistence;
using BookInventory.Infrastructure.Persistence.Configuration;

namespace BookInventory.Api.Hosting;

/// <summary>
/// P16: brings the database schema up to date before Kestrel accepts requests, in every environment (owner decision
/// 2026-10-26: Development and production both migrate on start). It is a hosted service and not code in
/// <c>Program.cs</c> for the same reason as <see cref="IamSeedHostedService"/>: tools that only build the host, such as
/// <c>dotnet ef</c>, never start hosted services, so generating a migration needs neither a database nor secrets.
/// A migration that fails stops startup: the host must never serve requests against a half-migrated schema.
/// </summary>
public sealed class DatabaseMigrationHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<DatabaseMigrationOptions> options,
    IHostEnvironment environment,
    ILogger<DatabaseMigrationHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.MigrateOnStartup)
        {
            logger.LogInformation("Database:MigrateOnStartup is off: pending migrations are not applied.");
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;

        var pending = (await database.GetPendingMigrationsAsync(cancellationToken)).ToList();
        if (pending.Count == 0)
        {
            logger.LogInformation("Database schema is up to date: no pending migration.");
            return;
        }

        var migrations = string.Join(", ", pending);
        if (!environment.IsDevelopment())
            logger.LogWarning("Applying {Count} pending migration(s) to the {Environment} database: {Migrations}",
                pending.Count, environment.EnvironmentName, migrations);
        else
            logger.LogInformation("Applying {Count} pending migration(s): {Migrations}", pending.Count, migrations);

        await database.MigrateAsync(cancellationToken);
        logger.LogInformation("Applied {Count} migration(s); the schema is up to date.", pending.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
