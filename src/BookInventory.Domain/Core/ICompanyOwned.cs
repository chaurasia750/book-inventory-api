namespace BookInventory.Domain.Core;

/// <summary>
/// Spec platform P18: an entity that holds one company's business data. The persistence layer stamps the company from
/// the session when the row is first saved (P20) and limits every read to the company of the session (P19); the audit
/// entry of a change names the company (P21). A row's company never changes.
/// </summary>
public interface ICompanyOwned : IAuditCompanyScope
{
    Guid CompanyId { get; }

    /// <summary>P20: called once by the persistence layer when the row is first saved; a second call or an empty id is refused.</summary>
    void AssignCompany(Guid companyId);
}
