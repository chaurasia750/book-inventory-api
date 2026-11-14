using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Company;

/// <summary>
/// One row of a company's status timeline (R8): from, to, reason, who and when. A child of <see cref="Company"/>, added
/// only by it, and never edited or deleted afterwards. <see cref="FromStatus"/> is null on the creation row.
/// </summary>
public class CompanyStatusHistory : TransactionDomainBase
{
    private CompanyStatusHistory() => Reason = string.Empty;

    public Guid CompanyId { get; private set; }

    public CompanyStatus? FromStatus { get; private set; }

    public CompanyStatus ToStatus { get; private set; }

    public string Reason { get; private set; }

    public Guid? ChangedBy { get; private set; }

    public DateTime ChangedAt { get; private set; }

    internal static CompanyStatusHistory Of(Guid companyId, CompanyStatus? from, CompanyStatus to, string reason, Guid actorId, DateTime utcNow) =>
        new()
        {
            CompanyId = companyId,
            FromStatus = from,
            ToStatus = to,
            Reason = reason,
            ChangedBy = actorId == Guid.Empty ? null : actorId,
            ChangedAt = utcNow
        };
}
