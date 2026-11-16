using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.counter_bill_line</c>: the copied snapshot and the amounts of each counter bill line (R43, R44, R13).</summary>
public sealed class CounterBillLineConfiguration() : ModuleEntityConfiguration<CounterBillLine>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CounterBillLine> builder)
    {
        builder.Property(l => l.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.HsnCode).HasMaxLength(8);
        builder.Property(l => l.GstRatePercent).HasPrecision(5, 2);
        builder.Property(l => l.PriceVersionName).HasMaxLength(30).IsRequired();
        foreach (var name in new[] { "MrpAmount", "ItemDiscountValue", "GrossAmount", "ItemDiscountAmount", "LineAmount", "TotalDiscountShareAmount", "TaxableAmount", "CgstAmount", "SgstAmount", "IgstAmount" })
            builder.Property(name).HasPrecision(18, 2);

        builder.HasIndex(l => new { l.CounterBillId, l.SortOrder });
    }
}
