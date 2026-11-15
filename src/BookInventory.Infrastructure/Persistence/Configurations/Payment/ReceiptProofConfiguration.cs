using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Payment;

/// <summary><c>payment.receipt_proof</c>: the optional proof files of a receipt (R20), kept as <c>bytea</c> in the database with the receipt.</summary>
public sealed class ReceiptProofConfiguration() : ModuleEntityConfiguration<ReceiptProof>("payment")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReceiptProof> builder)
    {
        builder.Property(p => p.FileName).HasMaxLength(ReceiptProof.FileNameMaxLength).IsRequired();
        builder.Property(p => p.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Content).IsRequired();
        builder.HasIndex(p => p.ReceiptId);
    }
}
