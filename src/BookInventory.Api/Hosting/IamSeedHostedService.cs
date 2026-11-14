using Microsoft.Extensions.Hosting;

using BookInventory.Infrastructure.Iam;

namespace BookInventory.Api.Hosting;

/// <summary>
/// Runs <see cref="IamSeeder"/> (R2) and then <see cref="SystemAccountSeeder"/> (R42) when the host starts, before Kestrel accepts requests. It is a hosted service and
/// not code in <c>Program.cs</c> on purpose: tools that only build the host, such as <c>dotnet ef</c>, never start
/// hosted services, so generating a migration does not need the seed secrets or a database.
/// </summary>
public sealed class IamSeedHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<IamSeeder>().SeedAsync(cancellationToken);
        await scope.ServiceProvider.GetRequiredService<SystemAccountSeeder>().SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
