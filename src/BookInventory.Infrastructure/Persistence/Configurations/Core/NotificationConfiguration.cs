using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Persistence.Configurations.Core;

/// <summary>
/// <c>core.notification</c> (spec core section 4): the outbox. The idempotency key is unique, which is what makes a
/// repeated request find the first row (N2); the two composite indexes are the worker's queries (N5). The attempt log
/// is reached only through the root, over its private backing field.
/// </summary>
public sealed class NotificationConfiguration() : ModuleEntityConfiguration<Notification>("core")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Notification> builder)
    {
        builder.Property(n => n.TemplateCode).HasMaxLength(NotificationTemplate.CodeMaxLength).IsRequired();
        builder.Property(n => n.Recipient).HasMaxLength(Notification.RecipientMaxLength).IsRequired();
        builder.Property(n => n.Variables).IsRequired();
        builder.Property(n => n.ProviderMessageId).HasMaxLength(256);
        builder.Property(n => n.ErrorMessage).HasMaxLength(2000);
        builder.Property(n => n.CorrelationId).HasMaxLength(64);
        builder.Property(n => n.IdempotencyKey).HasMaxLength(200).IsRequired();

        builder.HasIndex(n => n.IdempotencyKey).IsUnique();
        builder.HasIndex(n => new { n.Status, n.NextRetryOn });
        builder.HasIndex(n => new { n.Status, n.ScheduleOn });

        builder.HasMany(n => n.Logs)
            .WithOne()
            .HasForeignKey(l => l.NotificationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(n => n.Logs).HasField("_logs").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
