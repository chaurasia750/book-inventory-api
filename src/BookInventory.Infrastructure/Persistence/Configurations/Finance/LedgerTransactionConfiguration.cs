using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Finance;

/// <summary>
/// <c>finance.ledger_transaction</c>: company-owned (platform P18). The unique index on the document and the posting kind is the
/// real guard of R5: a document is posted once and reversed once, also when two requests race.
/// </summary>
public sealed class LedgerTransactionConfiguration() : ModuleEntityConfiguration<LedgerTransaction>("finance")
{
    protected override void ConfigureEntity(EntityTypeBuilder<LedgerTransaction> builder)
    {
        builder.Ignore(t => t.Document);
        builder.Property(t => t.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(t => t.Session).HasMaxLength(LedgerTransaction.SessionMaxLength);
        builder.Property(t => t.DocumentType).HasMaxLength(LedgerDocument.TypeMaxLength).IsRequired();
        builder.Property(t => t.DocumentNumber).HasMaxLength(LedgerDocument.NumberMaxLength).IsRequired();
        builder.Property(t => t.Narration).HasMaxLength(LedgerTransaction.NarrationMaxLength);

        builder.HasIndex(t => new { t.CompanyId, t.DocumentType, t.DocumentId, t.PostingKind })
            .IsUnique()
            .HasDatabaseName("ix_ledger_transaction_document_posting");

        builder.HasMany(t => t.Lines)
            .WithOne()
            .HasForeignKey(l => l.LedgerTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(t => t.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
