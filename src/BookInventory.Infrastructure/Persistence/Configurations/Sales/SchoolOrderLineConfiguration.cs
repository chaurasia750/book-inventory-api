using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.school_order_line</c>: the snapshot and amounts of each order line (R3, R7, R13).</summary>
public sealed class SchoolOrderLineConfiguration() : ModuleEntityConfiguration<SchoolOrderLine>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolOrderLine> builder)
    {
        builder.Ignore(l => l.ItemDiscount);
        builder.Ignore(l => l.Item);
        builder.Ignore(l => l.Price);
        builder.Ignore(l => l.CalculationInput);
        builder.Ignore(l => l.RemainingQuantity);
        builder.Property(l => l.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.HsnCode).HasMaxLength(8);
        builder.Property(l => l.GstRatePercent).HasPrecision(5, 2);
        builder.Property(l => l.PriceVersionName).HasMaxLength(30).IsRequired();
        foreach (var name in new[] { "MrpAmount", "RateAmount", "ItemDiscountValue", "GrossAmount", "ItemDiscountAmount", "LineAmount", "TotalDiscountShareAmount", "TaxableAmount", "CgstAmount", "SgstAmount", "IgstAmount", "ItemDiscountGiven" })
            builder.Property(name).HasPrecision(18, 2);

        builder.HasIndex(l => new { l.SchoolOrderId, l.SortOrder });
    }
}
