using BookInventory.Application.Iam.Companies;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Company;

/// <summary>R16: the session company's tax profile, read through the company store (not tracked).</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanyTaxProfile))]
public sealed class CompanyTaxProfile(ICompanyStore store, ICurrentCompany currentCompany) : ICompanyTaxProfile
{
    public async Task<CompanyTaxDetails?> CurrentAsync(CancellationToken cancellationToken = default)
    {
        if (currentCompany.CompanyId is not { } companyId)
            return null;

        var company = await store.FindForReadAsync(companyId, cancellationToken);
        return company is null ? null : new CompanyTaxDetails(company.State, company.Gstin);
    }
}
