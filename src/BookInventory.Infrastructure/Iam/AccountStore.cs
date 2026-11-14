using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Domain.Iam;
using BookInventory.Infrastructure.Persistence;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// The data side of the Super Admin account management. The queries that look past the hidden system-account filter are
/// deliberate: <see cref="FindActorsAsync"/> because the hidden account can be the actor of a history row and is then
/// shown under the fixed label (R48), and the three uniqueness lookups, because its user name and placeholder e-mail stay reserved (R52). No list, search, count or export of users comes through this class.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IAccountStore))]
public sealed class AccountStore : IAccountStore
{
    private readonly IRepository<AppUser> _users;
    private readonly IRepository<UserStatusHistory> _statusHistory;
    private readonly IRepository<UserRoleHistory> _roleHistory;

    public AccountStore(IUnitOfWork unitOfWork)
    {
        _users = unitOfWork.Repository<AppUser>();
        _statusHistory = unitOfWork.Repository<UserStatusHistory>();
        _roleHistory = unitOfWork.Repository<UserRoleHistory>();
    }

    public Task<AppUser?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default) =>
        _users.Table.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task AddAsync(AppUser user, CancellationToken cancellationToken = default) =>
        _users.AddAsync(user, cancellationToken);

    public Task AddAsync(UserStatusHistory history, CancellationToken cancellationToken = default) =>
        _statusHistory.AddAsync(history, cancellationToken);

    public Task AddAsync(UserRoleHistory history, CancellationToken cancellationToken = default) =>
        _roleHistory.AddAsync(history, cancellationToken);

    public Task<UserStatusHistory[]> FindStatusHistoryAsync(Guid userId, CancellationToken cancellationToken = default) =>
        _statusHistory.TableNoTracking
            .Where(h => h.UserId == userId)
            .OrderByDescending(h => h.ChangedAt)
            .ToArrayAsync(cancellationToken);

    public async Task<AppUser[]> FindActorsAsync(Guid[] userIds, CancellationToken cancellationToken = default) =>
        userIds.Length == 0
            ? []
            : await _users.TableNoTracking
                .IgnoreHiddenAccountFilter()
                .Where(u => userIds.Contains(u.Id))
                .ToArrayAsync(cancellationToken);

    public Task<bool> IsUserNameTakenAsync(string normalizedUserName, Guid? excludingUserId, CancellationToken cancellationToken = default) =>
        _users.TableNoTracking.IgnoreHiddenAccountFilter().AnyAsync(u => u.UserName == normalizedUserName && (excludingUserId == null || u.Id != excludingUserId), cancellationToken);

    public Task<bool> IsEmailTakenAsync(string normalizedEmail, Guid? excludingUserId, CancellationToken cancellationToken = default) =>
        _users.TableNoTracking.IgnoreHiddenAccountFilter().AnyAsync(u => u.Email == normalizedEmail && (excludingUserId == null || u.Id != excludingUserId), cancellationToken);

    public Task<bool> IsMobileTakenAsync(string normalizedMobile, Guid? excludingUserId, CancellationToken cancellationToken = default) =>
        _users.TableNoTracking.IgnoreHiddenAccountFilter().AnyAsync(u => u.Mobile == normalizedMobile && (excludingUserId == null || u.Id != excludingUserId), cancellationToken);

    public Task<AppUser[]> FindDueSuspensionsAsync(DateTime now, int take, CancellationToken cancellationToken = default) =>
        _users.Table
            .Where(u => u.Status == AccountStatus.Suspended && u.SuspendedUntil != null && u.SuspendedUntil <= now)
            .OrderBy(u => u.SuspendedUntil)
            .Take(take)
            .ToArrayAsync(cancellationToken);
}
