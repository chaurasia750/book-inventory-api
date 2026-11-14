using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.School;
using SchoolRoot = BookInventory.Domain.School.School;

namespace BookInventory.Infrastructure.Persistence.Configurations.School;

/// <summary>
/// <c>school.school</c>: company-owned (platform P18, P22): the code is unique per company, so the unique index starts
/// with <c>company_id</c>. The class list of R6 is a child table.
/// </summary>
public sealed class SchoolConfiguration() : ModuleEntityConfiguration<SchoolRoot>("school")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolRoot> builder)
    {
        builder.Property(s => s.Code).HasMaxLength(SchoolRoot.CodeMaxLength).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(SchoolRoot.NameMaxLength).IsRequired();
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.ContactPerson).HasMaxLength(200);
        builder.Property(s => s.Mobile).HasMaxLength(15);
        builder.Property(s => s.Email).HasMaxLength(254);
        builder.Property(s => s.Gstin).HasMaxLength(15);
        builder.Property(s => s.State).HasMaxLength(100).IsRequired();
        builder.Property(s => s.OpeningBalanceAmount).HasPrecision(18, 2);

        builder.HasIndex(s => new { s.CompanyId, s.Code }).IsUnique();
        builder.HasIndex(s => new { s.CompanyId, s.Status });
        builder.HasIndex(s => new { s.CompanyId, s.Name });

        builder.HasMany(s => s.Classes)
            .WithOne()
            .HasForeignKey(c => c.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(s => s.Classes).HasField("_classes").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
