using BookInventory.BuildingBlocks.Domain.Models;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Application.Company;

/// <summary>
/// The persistence port of the <c>company</c> module. Like the <c>iam</c> stores it has no save method: a write request
/// is committed by the request transaction. A company is found by id only; the reads return the aggregate with its
/// status timeline, and the list returns summaries (R9).
/// </summary>
public interface ICompanyStore
{
    Task AddAsync(CompanyRoot company, CancellationToken cancellationToken = default);

    /// <summary>The company a Super Admin is about to <em>change</em>: tracked, with its timeline. <c>null</c> for an unknown id.</summary>
    Task<CompanyRoot?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The company for reading: not tracked, with its timeline (the caller orders it).</summary>
    Task<CompanyRoot?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R3: whether another company already holds this normalised name; <paramref name="excludingId"/> is the company being edited.</summary>
    Task<bool> IsNameTakenAsync(string normalizedName, Guid? excludingId, CancellationToken cancellationToken = default);

    /// <summary>R63 of <c>iam</c>: whether the company exists and is active - one cheap lookup, nothing tracked.</summary>
    Task<bool> IsActiveAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R9: the paged, searchable, filterable list with a whitelisted sort.</summary>
    Task<IPagedList<CompanySummary>> SearchAsync(CompanySearchQuery query, CancellationToken cancellationToken = default);
}
