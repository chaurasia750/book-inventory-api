using BookInventory.BuildingBlocks.Domain.Models;

namespace BookInventory.Application.Iam;

/// <summary>
/// How other modules - and the <c>iam</c> user list - see users. It is a read-only port that hands out summaries and
/// never an <c>AppUser</c>, so no module outside <c>iam</c> can change an account by accident (R43).
/// <para>
/// Every method here goes through the named <c>HiddenAccount</c> query filter, so the hidden system account is absent
/// from every list, search, count and lookup, and an unknown id and the system account id are indistinguishable (both
/// answer "not found"). Nothing in this interface can return it.
/// </para>
/// </summary>
public interface IUserDirectory
{
    /// <summary>R39/R43: the detail of one user, or <c>null</c> for an unknown id <em>and</em> for the system account id.</summary>
    Task<UserDetail?> GetVisibleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R38/R43: the paged, searchable, filterable user list.</summary>
    Task<IPagedList<UserSummary>> SearchAsync(UserSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>
    /// R5/AC20: how many active <em>visible</em> Super Admins exist other than <paramref name="excludingUserId"/>. Zero
    /// means the account in question is the last one and must not be deactivated, suspended or demoted. The hidden
    /// system account is deliberately not counted.
    /// <para>
    /// It first takes a transaction-scoped lock shared by every caller, so two requests that each remove a Super Admin
    /// (each the other's last colleague) run one after the other and the second one sees the first one's result. The
    /// lock is released when the request transaction ends; call it inside that transaction, right before the change.
    /// </para>
    /// </summary>
    Task<int> LockAndCountOtherActiveVisibleSuperAdminsAsync(Guid excludingUserId, CancellationToken cancellationToken = default);
}
