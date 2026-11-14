using BookInventory.Infrastructure.Core;

namespace BookInventory.Api.Hosting;

/// <summary>
/// Runs <see cref="NotificationTemplateSeeder"/> when the host starts, before Kestrel accepts requests (spec core N13).
/// A hosted service and not code in <c>Program.cs</c> for the same reason as the Super Admin seed: tools that only build
/// the host, such as <c>dotnet ef</c>, never start hosted services.
/// </summary>
public sealed class NotificationTemplateSeedHostedService(IServiceScopeFactory scopeFactory) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<NotificationTemplateSeeder>().SeedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
