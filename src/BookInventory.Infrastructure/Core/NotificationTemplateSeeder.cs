using Microsoft.Extensions.Logging;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Application.Core;
using BookInventory.Domain.Core;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Spec core N13: at startup each module's default templates (<see cref="INotificationTemplateSource"/>) are inserted
/// when their code does not exist yet - and only then, so a template an operator has edited is never overwritten. A
/// database whose <c>core</c> schema is not migrated yet is skipped with a warning instead of failing the application.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class NotificationTemplateSeeder(
    INotificationStore store,
    IUnitOfWork unitOfWork,
    IEnumerable<INotificationTemplateSource> sources,
    ILogger<NotificationTemplateSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        HashSet<string> existing;
        try
        {
            existing = await store.FindTemplateCodesAsync(cancellationToken);
        }
        catch (Exception ex) when (PostgresErrors.IsUndefinedTable(ex))
        {
            logger.LogWarning("core schema not found, skipping the template seed. Apply the migrations before starting the application.");
            return;
        }

        var missing = sources
            .SelectMany(source => source.Templates)
            .Where(template => !existing.Contains(template.Code.Trim().ToUpperInvariant()))
            .ToArray();

        foreach (var template in missing)
            await store.AddAsync(NotificationTemplate.Create(template), cancellationToken);

        if (missing.Length == 0)
            return;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} default notification template(s): {Codes}.", missing.Length, string.Join(", ", missing.Select(t => t.Code)));
    }
}
