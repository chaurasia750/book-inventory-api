using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Company;

/// <summary>R13: what a Super Admin did with a company's workspace.</summary>
public enum CompanyEntryAction
{
    Entered,

    Left
}

/// <summary>
/// R12/R13: the insert-only record of one enter or leave of a company by a Super Admin: which company, who and when. It is
/// the trail of these actions until the <c>core</c> audit trail exists, and it is never edited or deleted.
/// </summary>
public class CompanyEntry : AggregateRoot
{
    private CompanyEntry()
    {
    }

    public Guid CompanyId { get; private set; }

    public Guid UserId { get; private set; }

    public CompanyEntryAction Action { get; private set; }

    public DateTime At { get; private set; }

    public static CompanyEntry Entered(Guid companyId, Guid userId, DateTime utcNow) => Of(companyId, userId, CompanyEntryAction.Entered, utcNow);

    public static CompanyEntry Left(Guid companyId, Guid userId, DateTime utcNow) => Of(companyId, userId, CompanyEntryAction.Left, utcNow);

    private static CompanyEntry Of(Guid companyId, Guid userId, CompanyEntryAction action, DateTime utcNow) =>
        new() { CompanyId = companyId, UserId = userId, Action = action, At = utcNow };
}
