using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Core;

/// <summary>
/// Spec platform P18-P21: the base of every aggregate root that belongs to one company. The company is assigned exactly
/// once, by the persistence layer from the session (never by a request); after that nothing changes it. Deriving from it
/// is all a module has to do for its rows to be stamped, filtered and audited by company.
/// </summary>
public abstract class CompanyOwnedRoot : AggregateRoot, ICompanyOwned
{
    public Guid CompanyId { get; private set; }

    Guid? IAuditCompanyScope.AuditCompanyId => CompanyId == Guid.Empty ? null : CompanyId;

    public void AssignCompany(Guid companyId) => CompanyId = CompanyAssignment.Apply(CompanyId, companyId);
}
