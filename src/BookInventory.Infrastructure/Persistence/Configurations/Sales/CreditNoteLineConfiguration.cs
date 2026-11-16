using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.credit_note_line</c>: the copied snapshot, the condition and the amounts of each credit note line (R54, R55, R59).</summary>
public sealed class CreditNoteLineConfiguration() : ModuleEntityConfiguration<CreditNoteLine>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CreditNoteLine> builder)
    {
        builder.Property(l => l.ClassName).HasMaxLength(50);
        builder.Property(l => l.ItemCode).HasMaxLength(30).IsRequired();
        builder.Property(l => l.ItemName).HasMaxLength(200).IsRequired();
        builder.Property(l => l.HsnCode).HasMaxLength(8);
        builder.Property(l => l.GstRatePercent).HasPrecision(5, 2);
        builder.Property(l => l.PriceVersionName).HasMaxLength(30).IsRequired();
        foreach (var name in new[]
                 {
                     "MrpAmount", "RateAmount", "ItemDiscountValue", "GrossAmount", "ItemDiscountAmount", "LineAmount", "TotalDiscountShareAmount",
                     "TaxableAmount", "CgstAmount", "SgstAmount", "IgstAmount"
                 })
            builder.Property(name).HasPrecision(18, 2);

        builder.HasIndex(l => new { l.CreditNoteId, l.SortOrder });
        builder.HasIndex(l => l.InvoiceLineId);
    }
}
