using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Company;

/// <summary><c>company.company_status_history</c>: the append-only timeline of R8, read newest first.</summary>
public sealed class CompanyStatusHistoryConfiguration() : ModuleEntityConfiguration<CompanyStatusHistory>("company")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CompanyStatusHistory> builder)
    {
        builder.Property(h => h.Reason).HasMaxLength(Domain.Company.Company.ReasonMaxLength).IsRequired();

        builder.HasIndex(h => new { h.CompanyId, h.ChangedAt });
    }
}
