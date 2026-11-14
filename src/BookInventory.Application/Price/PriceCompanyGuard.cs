using BookInventory.Application.Iam.Companies;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Price;

/// <summary>
/// P20: a price write needs a company in the session. A Super Admin who has not entered one is told so (<c>403 core.company_required</c>)
/// before anything else is looked up, so it is not mistaken for an unknown item. Written once for every price write.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class PriceCompanyGuard(ICurrentCompany currentCompany)
{
    public void Require()
    {
        if (currentCompany.CompanyId is null)
            throw new ForbiddenException(CoreMessages.CompanyRequired);
    }
}
