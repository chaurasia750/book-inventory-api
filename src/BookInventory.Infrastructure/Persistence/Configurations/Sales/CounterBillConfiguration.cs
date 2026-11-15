using BookInventory.BuildingBlocks.Persistence.Naming;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>
/// <c>sales.counter_bill</c>: company-owned; the number is unique per company (R42); the snapshots of R13; money <c>numeric(18,2)</c>.
/// The counter, school and student are plain ids with their snapshots, no foreign key across modules.
/// </summary>
public sealed class CounterBillConfiguration() : ModuleEntityConfiguration<CounterBill>("sales")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CounterBill> builder)
    {
        builder.Property(b => b.Number).HasMaxLength(40).IsRequired();
        builder.Property(b => b.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(b => b.SchoolCode).HasMaxLength(20).IsRequired();
        builder.Property(b => b.SchoolName).HasMaxLength(150).IsRequired();
        builder.Property(b => b.SchoolState).HasMaxLength(100);
        builder.Property(b => b.SchoolGstin).HasMaxLength(15);
        builder.Property(b => b.CompanyState).HasMaxLength(100);
        builder.Property(b => b.Session).HasMaxLength(Counter.SessionMaxLength).IsRequired();
        builder.Property(b => b.StudentName).HasMaxLength(Student.NameMaxLength).IsRequired();
        builder.Property(b => b.StudentClass).HasMaxLength(Student.ClassMaxLength).IsRequired();
        builder.Property(b => b.StudentSection).HasMaxLength(Student.SectionMaxLength);
        builder.Property(b => b.StudentRollNumber).HasMaxLength(Student.RollNumberMaxLength);
        builder.Property(b => b.StudentParentMobile).HasMaxLength(15);
        builder.Property(b => b.TotalDiscountValue).HasPrecision(18, 2);
        builder.Property(b => b.Remark).HasMaxLength(CounterBill.RemarkMaxLength);
        builder.Property(b => b.CancelReason).HasMaxLength(SchoolOrder.ReasonMaxLength);
        builder.ComplexProperty(b => b.CompanySnapshot, c =>
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
        builder.ComplexProperty(b => b.Totals, t =>
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

        builder.HasIndex(b => new { b.CompanyId, b.Number }).IsUnique();
        builder.HasIndex(b => new { b.CompanyId, b.BillDate });
        builder.HasIndex(b => new { b.CompanyId, b.StudentId });
        builder.HasIndex(b => new { b.CompanyId, b.CounterId });

        builder.HasMany(b => b.Lines).WithOne().HasForeignKey(l => l.CounterBillId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(b => b.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.HasMany(b => b.Payments).WithOne().HasForeignKey(p => p.CounterBillId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(b => b.Payments).HasField("_payments").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
