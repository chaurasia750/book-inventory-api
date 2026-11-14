using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Company;

/// <summary><c>company.company_entry</c>: the insert-only record of R12/R13 (a Super Admin entered or left a company), read by company and time.</summary>
public sealed class CompanyEntryConfiguration() : ModuleEntityConfiguration<CompanyEntry>("company")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CompanyEntry> builder)
    {
        builder.HasIndex(e => new { e.CompanyId, e.At });

        builder.HasOne<CompanyRoot>()
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
