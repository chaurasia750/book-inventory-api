using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Stock;

/// <summary>
/// <c>stock.stock_entry</c>: the insert-only movements of R4. Two unique indexes are the real guards: one opening per item and
/// stock type (R7), and one posting per document, item, stock type and kind (R11), so a retried document cannot count twice.
/// </summary>
public sealed class StockEntryConfiguration() : ModuleEntityConfiguration<StockEntry>("stock")
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockEntry> builder)
    {
        builder.Ignore(e => e.Change);
        builder.Property(e => e.DocumentType).HasMaxLength(StockDocument.TypeMaxLength);
        builder.Property(e => e.DocumentNumber).HasMaxLength(StockDocument.NumberMaxLength);
        builder.Property(e => e.Note).HasMaxLength(ItemStock.NoteMaxLength);

        builder.HasIndex(e => new { e.CompanyId, e.ItemId, e.MovementDate });
        builder.HasIndex(e => new { e.CompanyId, e.ItemId, e.StockType })
            .IsUnique()
            .HasFilter("movement_kind = 'OPENING'")
            .HasDatabaseName("ix_stock_entry_opening");
        builder.HasIndex(e => new { e.CompanyId, e.DocumentType, e.DocumentId, e.ItemId, e.StockType, e.MovementKind })
            .IsUnique()
            .HasFilter("document_id IS NOT NULL")
            .HasDatabaseName("ix_stock_entry_document_posting");
    }
}
