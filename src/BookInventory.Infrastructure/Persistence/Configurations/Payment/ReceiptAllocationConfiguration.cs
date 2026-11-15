using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Payment;

/// <summary>
/// <c>payment.receipt_allocation</c>: insert-only signed rows (R6): the allocated amount of an invoice is their sum. The invoice is a
/// plain id (no foreign key across schemas).
/// </summary>
public sealed class ReceiptAllocationConfiguration() : ModuleEntityConfiguration<ReceiptAllocation>("payment")
{
    protected override void ConfigureEntity(EntityTypeBuilder<ReceiptAllocation> builder)
    {
        builder.Property(a => a.Amount).HasPrecision(18, 2);
        builder.HasIndex(a => a.InvoiceId);
        builder.HasIndex(a => a.ReceiptId);
    }
}
