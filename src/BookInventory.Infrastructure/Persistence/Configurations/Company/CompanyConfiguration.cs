using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Company;

/// <summary>
/// <c>company.company</c>: the unique index on the normalised name is the real guard of R3 (two parallel creations
/// cannot both pass the lookup); the status timeline of R8 is a child table.
/// </summary>
public sealed class CompanyConfiguration() : ModuleEntityConfiguration<CompanyRoot>("company")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CompanyRoot> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(CompanyRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.NameNormalized).HasMaxLength(CompanyRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Mobile).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(254);
        builder.Property(c => c.Gstin).HasMaxLength(15);
        builder.Property(c => c.Pan).HasMaxLength(10);
        builder.Property(c => c.State).HasMaxLength(100);
        builder.Property(c => c.Tagline).HasMaxLength(200);
        builder.Property(c => c.StatusReason).HasMaxLength(CompanyRoot.ReasonMaxLength);

        builder.HasIndex(c => c.NameNormalized).IsUnique();
        builder.HasIndex(c => c.Status);

        builder.HasMany(c => c.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(c => c.StatusHistory).HasField("_statusHistory").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
