using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.School;

namespace BookInventory.Infrastructure.Persistence.Configurations.School;

/// <summary><c>school.school_code_counter</c>: one row per company (R15); the unique index on <c>company_id</c> is what makes it one.</summary>
public sealed class SchoolCodeCounterConfiguration() : ModuleEntityConfiguration<SchoolCodeCounter>("school")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolCodeCounter> builder) =>
        builder.HasIndex(c => c.CompanyId).IsUnique();
}
