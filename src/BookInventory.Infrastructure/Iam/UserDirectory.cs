using System.Linq.Expressions;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.BuildingBlocks.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using BookInventory.Application.Iam;
using BookInventory.Domain.Iam;

namespace BookInventory.Infrastructure.Iam;

/// <summary>
/// The only door to users for other modules (R43). Every query runs through the <c>HiddenAccount</c> filter, so the
/// system account cannot leak into a list, a search, a count or a detail lookup; the implementation does not even
/// have the option to switch that filter off.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IUserDirectory))]
public sealed class UserDirectory(IUnitOfWork unitOfWork, TimeProvider timeProvider) : IUserDirectory
{
    private readonly IRepository<AppUser> _users = unitOfWork.Repository<AppUser>();

    public Task<UserDetail?> GetVisibleByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _users.TableNoTracking
            .Where(u => u.Id == id)
            .Select(u => new UserDetail(
                u.Id, u.UserName, u.FullName, u.Email, u.Mobile, u.Role, u.Status, u.StatusReason,
                u.StatusChangedAt, u.StatusChangedBy, u.SuspendedUntil, u.MustChangePassword,
                u.LockedUntil != null && u.LockedUntil > timeProvider.GetUtcNow().UtcDateTime,
                u.LockedUntil, u.FailedLoginCount, u.PasswordChangedAt, u.LastLoginAt, u.CreatedAt, u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IPagedList<UserSummary>> SearchAsync(UserSearchQuery query, CancellationToken cancellationToken = default)
    {
        // Captured once so the lock filter and the reported flag cannot disagree, and so the value is a query parameter.
        var now = timeProvider.GetUtcNow().UtcDateTime;
        var pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
        var search = query.Search?.Trim();
        var role = query.Role;
        var companyId = query.CompanyId;
        var status = query.Status;
        var isLocked = query.IsLocked;

        var source = _users.TableNoTracking
            .Where(u => !role.HasValue || u.Role == role.Value)
            .Where(u => !companyId.HasValue || u.CompanyId == companyId.Value)
            .Where(u => !status.HasValue || u.Status == status.Value)
            .Where(u => !isLocked.HasValue
                        || (isLocked.Value
                            ? u.LockedUntil != null && u.LockedUntil > now
                            : u.LockedUntil == null || u.LockedUntil <= now))
            .Where(u => search == null
                        || u.FullName.Contains(search)
                        || u.UserName.Contains(search)
                        || (u.Email != null && u.Email.Contains(search))
                        || (u.Mobile != null && u.Mobile.Contains(search)));

        return await source
            .OrderBy(WhitelistedOrder(query.OrderBy))
            .Select(SummaryOf(now))
            .ToPagedListAsync(pageIndex, query.PageSize, cancellationToken);
    }

    public async Task<IReadOnlyList<UserSummary>> ListCompanyAccountsAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        await _users.TableNoTracking
            .Where(u => u.CompanyId == companyId)
            .OrderBy(u => u.CreatedAt)
            .Select(SummaryOf(timeProvider.GetUtcNow().UtcDateTime))
            .ToListAsync(cancellationToken);

    /// <summary>The one projection of a user summary, so the list and the company page show the same fields.</summary>
    private static Expression<Func<AppUser, UserSummary>> SummaryOf(DateTime now) =>
        u => new UserSummary(
            u.Id, u.UserName, u.FullName, u.Email, u.Mobile, u.Role, u.Status,
            u.LockedUntil != null && u.LockedUntil > now, u.LockedUntil, u.MustChangePassword,
            u.LastLoginAt, u.CreatedAt);

    /// <summary>
    /// The columns a caller may sort the user list by. The platform <c>OrderBy(string)</c> accepts any property of the
    /// entity (<c>PasswordHash</c> included), so the name is checked here first; anything else falls back to the default
    /// order and can never be used to probe a column.
    /// </summary>
    private static readonly string[] SortableColumns = ["UserName", "FullName", "Email", "Role", "Status", "CreatedAt", "LastLoginAt"];

    private static string? WhitelistedOrder(string? orderBy)
    {
        var parts = (orderBy ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var column = SortableColumns.FirstOrDefault(c => parts.Length > 0 && c.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
        if (column is null)
            return null;

        var ascending = parts.Length > 1 && parts[1].Equals("ASC", StringComparison.OrdinalIgnoreCase);
        return $"{column} {(ascending ? "ASC" : "DESC")}";
    }

    /// <summary>Arbitrary constant key of the advisory lock that serialises the removal of Super Admins (R5).</summary>
    private const long SuperAdminChangeLockKey = 5_100_001;

    public async Task<int> LockAndCountOtherActiveVisibleSuperAdminsAsync(Guid excludingUserId, CancellationToken cancellationToken = default)
    {
        await _users.ExecuteSqlAsync($"select pg_advisory_xact_lock({SuperAdminChangeLockKey})", cancellationToken);

        return await _users.TableNoTracking
            .Where(AppUser.VisibleActiveSuperAdmin)
            .CountAsync(u => u.Id != excludingUserId, cancellationToken);
    }
}
