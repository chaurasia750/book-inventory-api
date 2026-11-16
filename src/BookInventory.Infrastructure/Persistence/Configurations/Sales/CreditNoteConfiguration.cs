using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>
/// <c>sales.credit_note</c>: company-owned; the number is unique per company (R53); the snapshots of R59; money <c>numeric(18,2)</c>.
/// The school and the invoice are plain ids with their snapshots, no foreign key across modules.
/// </summary>
public sealed class CreditNoteConfiguration() : ModuleEntityConfiguration<CreditNote>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CreditNote> builder)
    {
        builder.Property(n => n.Number).HasMaxLength(40).IsRequired();
        builder.Property(n => n.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(n => n.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(n => n.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(n => n.SchoolState).HasMaxLength(100);
        builder.Property(n => n.SchoolGstin).HasMaxLength(15);
        builder.Property(n => n.SchoolAddress).HasMaxLength(500);
        builder.Property(n => n.CompanyState).HasMaxLength(100);
        builder.Property(n => n.InvoiceNumber).HasMaxLength(40);
        builder.Property(n => n.Session).HasMaxLength(Counter.SessionMaxLength).IsRequired();
        builder.Property(n => n.TotalDiscountValue).HasPrecision(18, 2);
        builder.Property(n => n.Remark).HasMaxLength(CreditNote.RemarkMaxLength);
        builder.Property(n => n.CancelReason).HasMaxLength(SchoolOrder.ReasonMaxLength);
        builder.MapCompanySnapshot(n => n.CompanySnapshot);
        builder.ComplexProperty(n => n.Totals, t =>
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

        builder.HasIndex(n => new { n.CompanyId, n.Number }).IsUnique();
        builder.HasIndex(n => new { n.CompanyId, n.SchoolId });
        builder.HasIndex(n => new { n.CompanyId, n.InvoiceId });

        builder.HasMany(n => n.Lines).WithOne().HasForeignKey(l => l.CreditNoteId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(n => n.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
