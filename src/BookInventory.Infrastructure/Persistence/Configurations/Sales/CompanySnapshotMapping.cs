using System.Linq.Expressions;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Sales;

/// <summary>The columns of the company's printed identity a sales document keeps (sales R13, R59), written once for the credit note.</summary>
internal static class CompanySnapshotMapping
{
    public static void MapCompanySnapshot<T>(this EntityTypeBuilder<T> builder, Expression<Func<T, CompanyInvoiceSnapshot?>> property)
        where T : class =>
        builder.ComplexProperty(property, c =>
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
}
