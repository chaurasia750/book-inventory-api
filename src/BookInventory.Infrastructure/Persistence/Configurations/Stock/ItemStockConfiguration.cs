using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Stock;

/// <summary>
/// <c>stock.item_stock</c>: company-owned (platform P18, P22), one row per item and company (unique index, D1). The item is a plain
/// id (no foreign key across schemas, R1). The entries of R4 are a child table; the concurrency token of the root serialises
/// parallel postings of one item.
/// </summary>
public sealed class ItemStockConfiguration() : ModuleEntityConfiguration<ItemStock>("stock")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ItemStock> builder)
    {
        builder.HasIndex(s => new { s.CompanyId, s.ItemId }).IsUnique();

        builder.HasMany(s => s.Entries)
            .WithOne()
            .HasForeignKey(e => e.ItemStockId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(s => s.Entries).HasField("_entries").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
