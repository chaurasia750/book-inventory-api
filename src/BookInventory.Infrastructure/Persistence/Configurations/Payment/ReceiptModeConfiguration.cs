using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Payment;

/// <summary><c>payment.receipt_mode</c>: one mode of a receipt (R5): the amount, the UPI or bank reference or the cheque details and its status (R10).</summary>
public sealed class ReceiptModeConfiguration() : ModuleEntityConfiguration<ReceiptMode>("payment")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReceiptMode> builder)
    {
        builder.Property(m => m.Amount).HasPrecision(18, 2);
        builder.Property(m => m.Reference).HasMaxLength(ReceiptMode.ReferenceMaxLength);
        builder.Property(m => m.ChequeNumber).HasMaxLength(ReceiptMode.ChequeNumberMaxLength);
        builder.Property(m => m.BankName).HasMaxLength(ReceiptMode.BankNameMaxLength);
        builder.Property(m => m.BounceReason).HasMaxLength(500);
        builder.HasIndex(m => m.ReceiptId);
    }
}
