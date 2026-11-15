using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;
using BookInventory.Domain.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>R20: the session company's printed identity, read through the company store (not tracked).</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanyInvoiceIdentity))]
public sealed class CompanyInvoiceIdentity(ICompanyStore store, ICurrentCompany currentCompany) : ICompanyInvoiceIdentity
{
    public async Task<CompanyPrintedIdentity?> CurrentAsync(CancellationToken cancellationToken = default)
    {
        if (currentCompany.CompanyId is not { } companyId)
            return null;

        var company = await store.FindForReadAsync(companyId, cancellationToken);
        return company is null ? null : ToIdentity(company);
    }

    private static CompanyPrintedIdentity ToIdentity(CompanyRoot company) =>
        new(
            company.Name, company.Address, company.Mobile, company.Email, company.Gstin, company.State,
            company.Pan, company.Tagline,
            company.Logo, company.LogoContentType, company.Signature, company.SignatureContentType,
            new CompanyBankDetails(
                company.BankAccountHolder, company.BankName, company.BankAccountNumber, company.BankIfsc,
                company.BankBranch, company.UpiId),
            new CompanyInvoiceSettings(company.InvoiceTerms, company.InvoiceFooter, company.PrintGstin, company.UsesItemDiscount));
}
