using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.School;

/// <summary><c>school.session_setting</c>: one row per company (R19); the unique index on the company is the guard.</summary>
public sealed class SessionSettingConfiguration() : ModuleEntityConfiguration<SessionSetting>("school")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SessionSetting> builder)
    {
        builder.Ignore(s => s.AprMar);
        builder.Ignore(s => s.JanDec);
        builder.HasIndex(s => s.CompanyId).IsUnique();
    }
}
