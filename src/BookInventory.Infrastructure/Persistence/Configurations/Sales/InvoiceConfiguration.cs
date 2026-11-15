using BookInventory.BuildingBlocks.Persistence.Naming;
using CompanyRoot = BookInventory.Domain.Company.Company;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary><c>sales.invoice</c>: company-owned; the number is unique per company (R10); the snapshot of R13; money <c>numeric(18,2)</c>.</summary>
public sealed class InvoiceConfiguration() : ModuleEntityConfiguration<Invoice>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<Invoice> builder)
    {
        builder.Ignore(i => i.Delivery);
        builder.Property(i => i.Number).HasMaxLength(40).IsRequired();
        builder.Property(i => i.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(i => i.OrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(i => i.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(i => i.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(i => i.SchoolState).HasMaxLength(100);
        builder.Property(i => i.SchoolAddress).HasMaxLength(500);
        builder.Property(i => i.SchoolGstin).HasMaxLength(15);
        builder.Property(i => i.CompanyState).HasMaxLength(100);
        builder.Property(i => i.Session).HasMaxLength(SchoolOrder.SessionMaxLength).IsRequired();
        builder.Property(i => i.TotalDiscountValue).HasPrecision(18, 2);
        builder.Property(i => i.CancelReason).HasMaxLength(SchoolOrder.ReasonMaxLength);
        builder.ComplexProperty(i => i.CompanySnapshot, c =>
        {
            c.Property(x => x.Name).HasColumnName("company_name").HasMaxLength(150);
            c.Property(x => x.Address).HasColumnName("company_address").HasMaxLength(500);
            c.Property(x => x.Mobile).HasColumnName("company_mobile").HasMaxLength(20);
            c.Property(x => x.Email).HasColumnName("company_email").HasMaxLength(254);
            c.Property(x => x.Gstin).HasColumnName("company_gstin").HasMaxLength(15);
            c.Property(x => x.Pan).HasColumnName("company_pan").HasMaxLength(10);
            c.Property(x => x.Tagline).HasColumnName("company_tagline").HasMaxLength(200);
            c.Property(x => x.State).HasColumnName("company_snapshot_state").HasMaxLength(100);
            c.Property(x => x.BankAccountHolder).HasColumnName("company_bank_account_holder").HasMaxLength(150);
            c.Property(x => x.BankName).HasColumnName("company_bank_name").HasMaxLength(150);
            c.Property(x => x.BankAccountNumber).HasColumnName("company_bank_account_number").HasMaxLength(50);
            c.Property(x => x.BankIfsc).HasColumnName("company_bank_ifsc").HasMaxLength(11);
            c.Property(x => x.BankBranch).HasColumnName("company_bank_branch").HasMaxLength(150);
            c.Property(x => x.UpiId).HasColumnName("company_upi_id").HasMaxLength(100);
            c.Property(x => x.Terms).HasColumnName("company_terms").HasMaxLength(CompanyRoot.TermsMaxLength);
            c.Property(x => x.Footer).HasColumnName("company_footer").HasMaxLength(CompanyRoot.FooterMaxLength);
            c.Property(x => x.PrintGstin).HasColumnName("company_print_gstin");
        });
        builder.ComplexProperty(i => i.Totals, t =>
        {
            t.Property(x => x.Subtotal).HasColumnName("subtotal_amount").HasPrecision(18, 2);
            t.Property(x => x.TotalDiscount).HasColumnName("total_discount_amount").HasPrecision(18, 2);
            t.Property(x => x.Taxable).HasColumnName("taxable_amount").HasPrecision(18, 2);
            t.Property(x => x.Cgst).HasColumnName("cgst_amount").HasPrecision(18, 2);
            t.Property(x => x.Sgst).HasColumnName("sgst_amount").HasPrecision(18, 2);
            t.Property(x => x.Igst).HasColumnName("igst_amount").HasPrecision(18, 2);
            t.Property(x => x.RoundOff).HasColumnName("round_off_amount").HasPrecision(18, 2);
            t.Property(x => x.GrandTotal).HasColumnName("grand_total_amount").HasPrecision(18, 2);
        });

        builder.HasIndex(i => new { i.CompanyId, i.Number }).IsUnique();
        builder.HasIndex(i => new { i.CompanyId, i.SchoolId, i.InvoiceDate });
        builder.HasIndex(i => i.SchoolOrderId);

        builder.HasMany(i => i.Lines).WithOne().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(i => i.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
