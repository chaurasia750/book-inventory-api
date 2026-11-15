using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.counter_bill_payment</c>: the Cash and UPI payments of a counter bill (R45); money <c>numeric(18,2)</c>.</summary>
public sealed class CounterBillPaymentConfiguration() : ModuleEntityConfiguration<CounterBillPayment>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CounterBillPayment> builder)
    {
        builder.Property(p => p.Amount).HasPrecision(18, 2);
        builder.Property(p => p.Reference).HasMaxLength(CounterBillPayment.ReferenceMaxLength);

        builder.HasIndex(p => p.CounterBillId);
    }
}
