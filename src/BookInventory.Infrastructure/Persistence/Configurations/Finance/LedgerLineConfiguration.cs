using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Finance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Finance;

/// <summary>
/// <c>finance.ledger_line</c>: the insert-only lines of R3. Money is <c>numeric(18,2)</c> (R8); a check keeps exactly one side
/// above zero. The party index serves the statement and the balances (R7, R9).
/// </summary>
public sealed class LedgerLineConfiguration() : ModuleEntityConfiguration<LedgerLine>("finance")
{
    protected override void ConfigureEntity(EntityTypeBuilder<LedgerLine> builder)
    {
        builder.Ignore(l => l.Party);
        builder.Property(l => l.DebitAmount).HasPrecision(18, 2);
        builder.Property(l => l.CreditAmount).HasPrecision(18, 2);
        builder.Property(l => l.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(l => l.Session).HasMaxLength(LedgerTransaction.SessionMaxLength);

        builder.ToTable(t => t.HasCheckConstraint(
            "ck_ledger_line_one_side", "(debit_amount > 0 AND credit_amount = 0) OR (credit_amount > 0 AND debit_amount = 0)"));

        builder.HasIndex(l => new { l.CompanyId, l.PartyType, l.PartyId, l.TransactionDate });
        builder.HasIndex(l => new { l.CompanyId, l.Account });
    }
}
