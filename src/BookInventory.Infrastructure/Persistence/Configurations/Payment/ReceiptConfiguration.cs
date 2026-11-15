using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Payment;

/// <summary>
/// <c>payment.receipt</c>: company-owned (R1); the number is unique per company (R8) and so is the client request id (R9); the
/// school is a plain id (school R16); the total is money with two decimals.
/// </summary>
public sealed class ReceiptConfiguration() : ModuleEntityConfiguration<Receipt>("payment")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Receipt> builder)
    {
        builder.Property(r => r.Number).HasMaxLength(Receipt.NumberMaxLength).IsRequired();
        builder.Property(r => r.FinancialYear).HasMaxLength(10).IsRequired();
        builder.Property(r => r.Remark).HasMaxLength(Receipt.RemarkMaxLength);
        builder.Property(r => r.TotalAmount).HasPrecision(18, 2);
        builder.Ignore(r => r.AllocatedAmount);
        builder.Ignore(r => r.AdvanceAmount);

        builder.HasIndex(r => new { r.CompanyId, r.Number }).IsUnique();
        builder.HasIndex(r => new { r.CompanyId, r.ClientRequestId }).IsUnique();
        builder.HasIndex(r => new { r.CompanyId, r.SchoolId, r.ReceiptDate });

        builder.HasMany(r => r.Modes).WithOne().HasForeignKey(m => m.ReceiptId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(r => r.Modes).HasField("_modes").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(r => r.Allocations).WithOne().HasForeignKey(a => a.ReceiptId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(r => r.Allocations).HasField("_allocations").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
