using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.School;

namespace BookInventory.Infrastructure.Persistence.Configurations.School;

/// <summary><c>school.school_class</c>: the ordered class list of R6; a name is unique inside its school among the live rows.</summary>
public sealed class SchoolClassConfiguration() : ModuleEntityConfiguration<SchoolClass>("school")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(SchoolClass.NameMaxLength).IsRequired();
        builder.Property(c => c.NameNormalized).HasMaxLength(SchoolClass.NameMaxLength).IsRequired();

        // A removed class is soft-deleted (platform P4) and stays in the table, so the name is unique among the live rows only;
        // the same name can be added again later.
        builder.HasIndex(c => new { c.SchoolId, c.NameNormalized }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(c => new { c.SchoolId, c.SortOrder });
    }
}
