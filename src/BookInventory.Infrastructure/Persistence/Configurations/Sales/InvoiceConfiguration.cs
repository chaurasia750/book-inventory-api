using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.invoice</c>: company-owned; the number is unique per company (R10); the snapshot of R13; money <c>numeric(18,2)</c>.</summary>
public sealed class InvoiceConfiguration() : ModuleEntityConfiguration<Invoice>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Invoice> builder)
    {
        builder.Ignore(i => i.Delivery);
        builder.Property(i => i.Number).HasMaxLength(40).IsRequired();
        builder.Property(i => i.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(i => i.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(i => i.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(i => i.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.SchoolState).HasMaxLength(100);
        builder.Property(i => i.SchoolGstin).HasMaxLength(15);
        builder.Property(i => i.CompanyState).HasMaxLength(100);
        builder.Property(i => i.Session).HasMaxLength(SchoolOrder.SessionMaxLength).IsRequired();
        builder.Property(i => i.TotalDiscountValue).HasPrecision(18, 2);
        builder.Property(i => i.CancelReason).HasMaxLength(SchoolOrder.ReasonMaxLength);
        builder.ComplexProperty(i => i.Totals, t =>
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

        builder.HasIndex(i => new { i.CompanyId, i.Number }).IsUnique();
        builder.HasIndex(i => new { i.CompanyId, i.SchoolId, i.InvoiceDate });
        builder.HasIndex(i => i.SchoolOrderId);

        builder.HasMany(i => i.Lines).WithOne().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(i => i.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
