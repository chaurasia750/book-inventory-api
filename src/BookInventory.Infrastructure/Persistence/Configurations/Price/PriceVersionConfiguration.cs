using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Price;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Price;

/// <summary>
/// <c>price.price_version</c>: company-owned (platform P18, P22). The item is a plain id (no foreign key across schemas, R1). The
/// normalised name is unique per item among the live rows (R2); money is <c>numeric(18,2)</c> (R17).
/// </summary>
public sealed class PriceVersionConfiguration() : ModuleEntityConfiguration<PriceVersion>("price")
{
    protected override void ConfigureEntity(EntityTypeBuilder<PriceVersion> builder)
    {
        builder.Property(v => v.VersionName).HasMaxLength(PriceVersion.NameMaxLength).IsRequired();
        builder.Property(v => v.VersionNameNormalized).HasMaxLength(PriceVersion.NameMaxLength).IsRequired();
        builder.Property(v => v.MrpAmount).HasPrecision(18, 2);
        builder.Property(v => v.SaleAmount).HasPrecision(18, 2);
        builder.Property(v => v.PurchaseAmount).HasPrecision(18, 2);

        builder.HasIndex(v => new { v.CompanyId, v.ItemId, v.VersionNameNormalized }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(v => new { v.CompanyId, v.ItemId, v.EffectiveFrom });
    }
}
