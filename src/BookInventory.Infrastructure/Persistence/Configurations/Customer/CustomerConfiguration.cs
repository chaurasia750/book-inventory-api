using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomerRoot = BookInventory.Domain.Customer.Customer;

namespace BookInventory.Infrastructure.Persistence.Configurations.Customer;

/// <summary>
/// <c>customer.customer</c>: company-owned (platform P18, P22): the code and the mobile are unique per company, so the unique
/// indexes start with <c>company_id</c> and cover live rows only (R3, R10). The school is a plain id, no foreign key across schemas.
/// </summary>
public sealed class CustomerConfiguration() : ModuleEntityConfiguration<CustomerRoot>("customer")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustomerRoot> builder)
    {
        builder.Property(c => c.Code).HasMaxLength(CustomerRoot.CodeMaxLength).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(CustomerRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.NameNormalized).HasMaxLength(CustomerRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.Mobile).HasMaxLength(15).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(CustomerRoot.AddressMaxLength);
        builder.Property(c => c.ClassName).HasMaxLength(CustomerRoot.ClassMaxLength);
        builder.Property(c => c.OpeningBalanceAmount).HasPrecision(18, 2);
        builder.Property(c => c.CreditLimitAmount).HasPrecision(18, 2);

        builder.HasIndex(c => new { c.CompanyId, c.Code }).IsUnique();
        builder.HasIndex(c => new { c.CompanyId, c.Mobile }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(c => new { c.CompanyId, c.NameNormalized });
        builder.HasIndex(c => new { c.CompanyId, c.SchoolId });
    }
}
