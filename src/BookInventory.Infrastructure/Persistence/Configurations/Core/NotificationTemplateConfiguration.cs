using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Persistence.Configurations.Core;

/// <summary><c>core.notification_template</c>: the code is unique, so a template is found by it and never seeded twice (N13).</summary>
public sealed class NotificationTemplateConfiguration() : ModuleEntityConfiguration<NotificationTemplate>("core")
{
    protected override void ConfigureEntity(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.Property(t => t.Code).HasMaxLength(NotificationTemplate.CodeMaxLength).IsRequired();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Subject).HasMaxLength(500).IsRequired();
        builder.Property(t => t.Body).IsRequired();
        builder.Property(t => t.LanguageCode).HasMaxLength(10).IsRequired();
        builder.Property(t => t.VariablesHint).HasMaxLength(1000);

        builder.HasIndex(t => t.Code).IsUnique();
    }
}
