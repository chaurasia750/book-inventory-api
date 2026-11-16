using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.counter</c>: company-owned (platform P18); the school is a plain id, no foreign key across schemas.</summary>
public sealed class CounterConfiguration() : ModuleEntityConfiguration<Counter>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Counter> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(Counter.NameMaxLength).IsRequired();
        builder.Property(c => c.Session).HasMaxLength(Counter.SessionMaxLength).IsRequired();

        builder.HasIndex(c => new { c.CompanyId, c.SchoolId });
        builder.HasIndex(c => new { c.CompanyId, c.Status });
    }
}
