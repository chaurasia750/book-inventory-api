using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>
/// <c>sales.school_order</c>: company-owned (platform P18); the number is unique per company (R2); the school is a plain id with its
/// snapshot (R13); money <c>numeric(18,2)</c>, percentages <c>numeric(5,2)</c> (R17).
/// </summary>
public sealed class SchoolOrderConfiguration() : ModuleEntityConfiguration<SchoolOrder>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolOrder> builder)
    {
        builder.Ignore(o => o.TotalDiscount);
        builder.Property(o => o.Number).HasMaxLength(40).IsRequired();
        builder.Property(o => o.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(o => o.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(o => o.SchoolState).HasMaxLength(100);
        builder.Property(o => o.Session).HasMaxLength(SchoolOrder.SessionMaxLength).IsRequired();
        builder.Property(o => o.TotalDiscountValue).HasPrecision(18, 2);
        builder.Property(o => o.TotalDiscountGiven).HasPrecision(18, 2);
        builder.Property(o => o.Remark).HasMaxLength(SchoolOrder.RemarkMaxLength);
        builder.Property(o => o.CloseReason).HasMaxLength(SchoolOrder.ReasonMaxLength);
        builder.ComplexProperty(o => o.Totals, t =>
        {
            t.Property(x => x.Subtotal).HasColumnName("subtotal_amount").HasPrecision(18, 2);
            t.Property(x => x.TotalDiscount).HasColumnName("total_discount_amount").HasPrecision(18, 2);
            t.Property(x => x.Taxable).HasColumnName("taxable_amount").HasPrecision(18, 2);
            t.Property(x => x.Cgst).HasColumnName("cgst_amount").HasPrecision(18, 2);
            t.Property(x => x.Sgst).HasColumnName("sgst_amount").HasPrecision(18, 2);
            t.Property(x => x.Igst).HasColumnName("igst_amount").HasPrecision(18, 2);
            t.Property(x => x.RoundOff).HasColumnName("round_off_amount").HasPrecision(18, 2);
            t.Property(x => x.GrandTotal).HasColumnName("grand_total_amount").HasPrecision(18, 2);
        });

        builder.HasIndex(o => new { o.CompanyId, o.Number }).IsUnique();
        builder.HasIndex(o => new { o.CompanyId, o.SchoolId, o.OrderDate });
        builder.HasIndex(o => new { o.CompanyId, o.Status });

        builder.HasMany(o => o.Lines).WithOne().HasForeignKey(l => l.SchoolOrderId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(o => o.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
