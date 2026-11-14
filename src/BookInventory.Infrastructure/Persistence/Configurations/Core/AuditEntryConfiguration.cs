using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Core;

namespace BookInventory.Infrastructure.Persistence.Configurations.Core;

/// <summary>
/// <c>core.audit_entry</c>: the insert-only trail of spec core A1-A6. Read three ways, hence three indexes: by company and
/// time (the Owner's list and the Super Admin's company filter), by entity and time (the history of one row), and by actor
/// and time. The company is a plain id with no foreign key (AD3).
/// </summary>
public sealed class AuditEntryConfiguration() : ModuleEntityConfiguration<AuditEntry>("core")
{
    protected override void ConfigureEntity(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.Property(e => e.EntityType).HasMaxLength(AuditEntry.EntityTypeMaxLength).IsRequired();
        builder.Property(e => e.ActorName).HasMaxLength(AuditEntry.ActorNameMaxLength);

        builder.HasIndex(e => new { e.CompanyId, e.CreatedAt }).IsDescending(false, true);
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.CreatedAt }).IsDescending(false, false, true);
        builder.HasIndex(e => new { e.ActorId, e.CreatedAt }).IsDescending(false, true);
    }
}
