using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Purchase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Purchase;

/// <summary><c>purchase.vendor_return_line</c>: the copied item, where it left from, the rate, the quantity and the amount of each line (R4, R10).</summary>
public sealed class VendorReturnLineConfiguration() : ModuleEntityConfiguration<VendorReturnLine>("purchase")
{
    protected override void ConfigureEntity(EntityTypeBuilder<VendorReturnLine> builder)
    {
        builder.Property(l => l.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.RateAmount).HasPrecision(18, 2);
        builder.Property(l => l.LineAmount).HasPrecision(18, 2);

        builder.HasIndex(l => new { l.VendorReturnId, l.SortOrder });
    }
}
