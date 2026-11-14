using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Item;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Item;

/// <summary>
/// <c>item.item_barcode</c>: the barcodes of R6. A barcode is unique per company among the live rows (platform P22); a removed
/// barcode is soft-deleted (platform P4) and stays in the table, so it can be given to another item.
/// </summary>
public sealed class ItemBarcodeConfiguration() : ModuleEntityConfiguration<ItemBarcode>("item")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ItemBarcode> builder)
    {
        builder.Property(b => b.Barcode).HasMaxLength(ItemBarcode.MaxLength).IsRequired();

        builder.HasIndex(b => new { b.CompanyId, b.Barcode }).IsUnique().HasFilter("is_deleted = false");
        builder.HasIndex(b => b.ItemId);
    }
}
