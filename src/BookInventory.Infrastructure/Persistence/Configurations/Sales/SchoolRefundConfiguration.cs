using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>
/// <c>sales.school_refund</c>: company-owned; the number is unique per company (R64); money <c>numeric(18,2)</c>. The school and the
/// credit note are plain ids with their snapshots, no foreign key across modules.
/// </summary>
public sealed class SchoolRefundConfiguration() : ModuleEntityConfiguration<SchoolRefund>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolRefund> builder)
    {
        builder.Property(r => r.Number).HasMaxLength(40).IsRequired();
        builder.Property(r => r.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(r => r.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(r => r.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(r => r.CreditNoteNumber).HasMaxLength(40);
        builder.Property(r => r.Reference).HasMaxLength(SchoolRefund.ReferenceMaxLength);
        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Remark).HasMaxLength(SchoolRefund.RemarkMaxLength);
        builder.Property(r => r.CancelReason).HasMaxLength(SchoolOrder.ReasonMaxLength);

        builder.HasIndex(r => new { r.CompanyId, r.Number }).IsUnique();
        builder.HasIndex(r => new { r.CompanyId, r.SchoolId });
        builder.HasIndex(r => new { r.CompanyId, r.CreditNoteId });
    }
}
