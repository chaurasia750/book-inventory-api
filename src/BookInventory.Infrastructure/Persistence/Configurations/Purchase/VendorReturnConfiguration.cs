using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Purchase;
using BookInventory.Infrastructure.Persistence.Configurations.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Purchase;

/// <summary>
/// <c>purchase.vendor_return</c>: company-owned; the number is unique per company (R3); the snapshot of R10; money <c>numeric(18,2)</c>.
/// The vendor is a plain id with its snapshot, no foreign key across modules.
/// </summary>
public sealed class VendorReturnConfiguration() : ModuleEntityConfiguration<VendorReturn>("purchase")
{
    protected override void ConfigureEntity(EntityTypeBuilder<VendorReturn> builder)
    {
        builder.Property(r => r.Number).HasMaxLength(40).IsRequired();
        builder.Property(r => r.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(r => r.VendorCode).HasMaxLength(20).IsRequired();
        builder.Property(r => r.VendorName).HasMaxLength(150).IsRequired();
        builder.Property(r => r.Remark).HasMaxLength(VendorReturn.RemarkMaxLength);
        builder.Property(r => r.CancelReason).HasMaxLength(VendorReturn.ReasonMaxLength);
        builder.Property(r => r.SubtotalAmount).HasPrecision(18, 2);
        builder.Property(r => r.RoundOffAmount).HasPrecision(18, 2);
        builder.Property(r => r.GrandTotalAmount).HasPrecision(18, 2);
        builder.MapCompanySnapshot(r => r.CompanySnapshot);

        builder.HasIndex(r => new { r.CompanyId, r.Number }).IsUnique();
        builder.HasIndex(r => new { r.CompanyId, r.VendorId });
        builder.HasIndex(r => new { r.CompanyId, r.ReturnDate });

        builder.HasMany(r => r.Lines).WithOne().HasForeignKey(l => l.VendorReturnId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(r => r.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
