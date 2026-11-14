using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using BookInventory.Application.Iam.Companies;

namespace BookInventory.Application.Company;

/// <summary>R63 of <c>iam</c>: answers whether a company may be used. <c>iam</c> defines the port; the company module implements it.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ICompanyStatusReader))]
public sealed class CompanyStatusReader(ICompanyStore store) : ICompanyStatusReader
{
    public Task<bool> IsActiveAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        store.IsActiveAsync(companyId, cancellationToken);
}
