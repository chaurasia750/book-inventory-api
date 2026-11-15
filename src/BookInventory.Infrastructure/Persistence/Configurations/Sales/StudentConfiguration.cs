using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>
/// <c>sales.student</c>: company-owned (platform P18). A school's student is unique by normalised name and class (spec sales R40;
/// the section never decides, Q26), over live rows only.
/// </summary>
public sealed class StudentConfiguration() : ModuleEntityConfiguration<Student>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Student> builder)
    {
        builder.Property(s => s.Name).HasMaxLength(Student.NameMaxLength).IsRequired();
        builder.Property(s => s.NameNormalized).HasMaxLength(Student.NameMaxLength).IsRequired();
        builder.Property(s => s.ClassName).HasMaxLength(Student.ClassMaxLength).IsRequired();
        builder.Property(s => s.Section).HasMaxLength(Student.SectionMaxLength);
        builder.Property(s => s.RollNumber).HasMaxLength(Student.RollNumberMaxLength);
        builder.Property(s => s.ParentMobile).HasMaxLength(15);

        builder.HasIndex(s => new { s.CompanyId, s.SchoolId, s.NameNormalized, s.ClassName }).IsUnique().HasFilter("is_deleted = false");
    }
}
