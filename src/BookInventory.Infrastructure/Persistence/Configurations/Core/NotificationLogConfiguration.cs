using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Persistence.Configurations.Core;

/// <summary><c>core.notification_log</c>: one row per send attempt (N6), insert-only.</summary>
public sealed class NotificationLogConfiguration() : ModuleEntityConfiguration<NotificationLog>("core")
{
    protected override void ConfigureEntity(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.Property(l => l.Response).HasMaxLength(2000).IsRequired();
        builder.Property(l => l.ErrorMessage).HasMaxLength(2000);
        builder.Property(l => l.ProviderMessageId).HasMaxLength(256);

        builder.HasIndex(l => new { l.NotificationId, l.AttemptNo });
    }
}
