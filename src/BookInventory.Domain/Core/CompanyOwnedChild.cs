using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Core;

/// <summary>
/// Spec platform P18: the base of a child entity inside a company-owned aggregate (a school's class). Its table carries
/// its own <c>company_id</c>, stamped by the persistence layer like the root's, so the child is filtered by company too and
/// its audit entries name the company (P21), which is what lets an Owner see them.
/// </summary>
public abstract class CompanyOwnedChild : DomainBase, ICompanyOwned
{
    public Guid CompanyId { get; private set; }

    Guid? IAuditCompanyScope.AuditCompanyId => CompanyId == Guid.Empty ? null : CompanyId;

    public void AssignCompany(Guid companyId) => CompanyId = CompanyAssignment.Apply(CompanyId, companyId);
}
