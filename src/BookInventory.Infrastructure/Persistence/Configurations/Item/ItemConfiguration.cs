using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Infrastructure.Persistence.Configurations.Item;

/// <summary>
/// <c>item.item</c>: company-owned (platform P18, P22): the code is unique per company, so the unique index starts with
/// <c>company_id</c>. The vendor is a plain id (no foreign key across schemas, R5). The barcodes of R6 are a child table.
/// </summary>
public sealed class ItemConfiguration() : ModuleEntityConfiguration<ItemRoot>("item")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ItemRoot> builder)
    {
        builder.Property(i => i.Code).HasMaxLength(ItemRoot.CodeMaxLength).IsRequired();
        builder.Property(i => i.Name).HasMaxLength(ItemRoot.NameMaxLength).IsRequired();
        builder.Property(i => i.ClassName).HasMaxLength(ItemRoot.ClassMaxLength);
        builder.Property(i => i.Subject).HasMaxLength(ItemRoot.SubjectMaxLength);
        builder.Property(i => i.HsnCode).HasMaxLength(8);
        builder.Property(i => i.GstRatePercent).HasPrecision(5, 2);

        builder.HasIndex(i => new { i.CompanyId, i.Code }).IsUnique();
        builder.HasIndex(i => new { i.CompanyId, i.Status });
        builder.HasIndex(i => new { i.CompanyId, i.Category });
        builder.HasIndex(i => new { i.CompanyId, i.VendorId });

        builder.HasMany(i => i.Barcodes)
            .WithOne()
            .HasForeignKey(b => b.ItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(i => i.Barcodes).HasField("_barcodes").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
