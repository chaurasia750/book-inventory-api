using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam;

/// <summary>
/// A user as other modules and the user list see them. The system account is never one of these (R43).
/// </summary>
public sealed record UserSummary(
    Guid Id,
    string UserName,
    string FullName,
    string? Email,
    string? Mobile,
    Role Role,
    AccountStatus Status,
    bool IsLocked,
    DateTime? LockedUntil,
    bool MustChangePassword,
    DateTime? LastLoginAt,
    DateTime CreatedAt);

/// <summary>The user detail page data of R39 (without the history and sessions, which are separate queries).</summary>
public sealed record UserDetail(
    Guid Id,
    string UserName,
    string FullName,
    string? Email,
    string? Mobile,
    Role Role,
    AccountStatus Status,
    string? StatusReason,
    DateTime? StatusChangedAt,
    Guid? StatusChangedBy,
    DateTime? SuspendedUntil,
    bool MustChangePassword,
    bool IsLocked,
    DateTime? LockedUntil,
    int FailedLoginCount,
    DateTime? PasswordChangedAt,
    DateTime? LastLoginAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

/// <summary>The list filters and paging of R38. <see cref="OrderBy"/> is whitelisted column names only.</summary>
public sealed record UserSearchQuery
{
    /// <summary>Free text over full name, user name, email and mobile.</summary>
    public string? Search { get; init; }

    public Role? Role { get; init; }

    public AccountStatus? Status { get; init; }

    /// <summary>Filter on the automatic lockout of R9, not on the account status.</summary>
    public bool? IsLocked { get; init; }

    /// <summary>Whitelisted sort, for example <c>"LastLoginAt DESC"</c> or <c>"CreatedAt DESC"</c>.</summary>
    public string? OrderBy { get; init; } = "CreatedAt DESC";

    public int PageIndex { get; init; } = 1;

    public int PageSize { get; init; } = 20;
}
