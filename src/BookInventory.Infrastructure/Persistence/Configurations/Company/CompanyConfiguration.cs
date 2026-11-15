using BookInventory.BuildingBlocks.Persistence.Naming;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Infrastructure.Persistence.Configurations.Company;

/// <summary>
/// <c>company.company</c>: the unique index on the normalised name is the real guard of R3 (two parallel creations
/// cannot both pass the lookup); the status timeline of R8 is a child table. The invoice identity of R17-R20 lives on the
/// same row: the two images as <c>bytea</c>, the bank account, the invoice settings and <c>print_gstin</c> (default true).
/// </summary>
public sealed class CompanyConfiguration() : ModuleEntityConfiguration<CompanyRoot>("company")
{
    protected override void ConfigureEntity(EntityTypeBuilder<CompanyRoot> builder)
    {
        builder.Property(c => c.Name).HasMaxLength(CompanyRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.NameNormalized).HasMaxLength(CompanyRoot.NameMaxLength).IsRequired();
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.Mobile).HasMaxLength(20);
        builder.Property(c => c.Email).HasMaxLength(254);
        builder.Property(c => c.Gstin).HasMaxLength(15);
        builder.Property(c => c.Pan).HasMaxLength(10);
        builder.Property(c => c.State).HasMaxLength(100);
        builder.Property(c => c.Tagline).HasMaxLength(200);
        builder.Property(c => c.StatusReason).HasMaxLength(CompanyRoot.ReasonMaxLength);

        builder.Property(c => c.Logo).HasColumnType("bytea");
        builder.Property(c => c.LogoContentType).HasMaxLength(100);
        builder.Property(c => c.Signature).HasColumnType("bytea");
        builder.Property(c => c.SignatureContentType).HasMaxLength(100);
        builder.Property(c => c.BankAccountHolder).HasMaxLength(CompanyRoot.BankAccountHolderMaxLength);
        builder.Property(c => c.BankName).HasMaxLength(CompanyRoot.BankNameMaxLength);
        builder.Property(c => c.BankAccountNumber).HasMaxLength(CompanyRoot.BankAccountNumberMaxLength);
        builder.Property(c => c.BankIfsc).HasMaxLength(CompanyRoot.BankIfscMaxLength);
        builder.Property(c => c.BankBranch).HasMaxLength(CompanyRoot.BankBranchMaxLength);
        builder.Property(c => c.UpiId).HasMaxLength(CompanyRoot.UpiIdMaxLength);
        builder.Property(c => c.InvoiceTerms).HasMaxLength(CompanyRoot.TermsMaxLength);
        builder.Property(c => c.InvoiceFooter).HasMaxLength(CompanyRoot.FooterMaxLength);
        builder.Property(c => c.PrintGstin).HasDefaultValue(true);

        builder.HasIndex(c => c.NameNormalized).IsUnique();
        builder.HasIndex(c => c.Status);

        builder.HasMany(c => c.StatusHistory)
            .WithOne()
            .HasForeignKey(h => h.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(c => c.StatusHistory).HasField("_statusHistory").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
