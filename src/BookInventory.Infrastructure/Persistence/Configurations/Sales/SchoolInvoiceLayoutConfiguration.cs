using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.school_invoice_layout</c>: company-owned; one live row per school (R26), made on the first change.</summary>
public sealed class SchoolInvoiceLayoutConfiguration() : ModuleEntityConfiguration<SchoolInvoiceLayout>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<SchoolInvoiceLayout> builder) =>
        builder.HasIndex(l => new { l.CompanyId, l.SchoolId }).IsUnique().HasFilter("is_deleted = false");
}
