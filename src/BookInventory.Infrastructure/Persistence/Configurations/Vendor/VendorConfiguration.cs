using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VendorRoot = BookInventory.Domain.Vendor.Vendor;

namespace BookInventory.Infrastructure.Persistence.Configurations.Vendor;

/// <summary>
/// <c>vendor.vendor</c>: company-owned (platform P18, P22): the code and the normalised name are unique per company, so the
/// unique indexes start with <c>company_id</c>; the name index covers live rows only (R4).
/// </summary>
public sealed class VendorConfiguration() : ModuleEntityConfiguration<VendorRoot>("vendor")
{
    protected override void ConfigureEntity(EntityTypeBuilder<VendorRoot> builder)
    {
        builder.Property(v => v.Code).HasMaxLength(VendorRoot.CodeMaxLength).IsRequired();
        builder.Property(v => v.Name).HasMaxLength(VendorRoot.NameMaxLength).IsRequired();
        builder.Property(v => v.NameNormalized).HasMaxLength(VendorRoot.NameMaxLength).IsRequired();
        builder.Property(v => v.Address).HasMaxLength(500);
        builder.Property(v => v.ContactPerson).HasMaxLength(200);
        builder.Property(v => v.Mobile).HasMaxLength(15);
        builder.Property(v => v.Email).HasMaxLength(254);
        builder.Property(v => v.Gstin).HasMaxLength(15);
        builder.Property(v => v.State).HasMaxLength(100);
        builder.Property(v => v.BankAccountHolder).HasMaxLength(VendorRoot.BankTextMaxLength);
        builder.Property(v => v.BankName).HasMaxLength(VendorRoot.BankTextMaxLength);
        builder.Property(v => v.BankBranch).HasMaxLength(VendorRoot.BankTextMaxLength);
        builder.Property(v => v.BankAccountNumber).HasMaxLength(18);
        builder.Property(v => v.BankIfsc).HasMaxLength(11);
        builder.Property(v => v.OpeningBalanceAmount).HasPrecision(18, 2);

        builder.HasIndex(v => new { v.CompanyId, v.Code }).IsUnique();
        builder.HasIndex(v => new { v.CompanyId, v.NameNormalized }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(v => new { v.CompanyId, v.Status });
    }
}
