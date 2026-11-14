using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Core;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Core;

/// <summary><c>core.company_code_series</c>: one row per company and prefix (platform P22, P23); the unique index is what makes it one.</summary>
public sealed class CompanyCodeSeriesConfiguration() : ModuleEntityConfiguration<CompanyCodeSeries>("core")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CompanyCodeSeries> builder)
    {
        builder.Property(s => s.Prefix).HasMaxLength(CompanyCodeSeries.PrefixMaxLength).IsRequired();
        builder.HasIndex(s => new { s.CompanyId, s.Prefix }).IsUnique();
    }
}
