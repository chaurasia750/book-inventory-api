using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Accounts;

/// <summary>
/// R57/D8: the answer of <c>POST users</c>. The temporary password is in here <em>once</em> - it is never stored in the
/// clear and there is no endpoint that can hand it out again, so a lost one is replaced by "send a reset link" (R36).
/// </summary>
public sealed record CreatedStaffResponse(
    Guid Id,
    string UserName,
    string FullName,
    string? Email,
    string? Mobile,
    Role Role,
    AccountStatus Status,
    bool MustChangePassword,
    string TemporaryPassword);

/// <summary>
/// One row of the status timeline of R33, as the detail page shows it. A row with <em>no</em> <c>ChangedByName</c> is
/// either the automatic lift of an expired suspension (R32, no actor) or an action of the hidden system account (R48),
/// which is shown under a fixed label with neither its name nor its id; the screen decides the wording, the API does
/// not send one.
/// </summary>
public sealed record UserStatusHistoryItem(
    Guid Id,
    AccountStatus FromStatus,
    AccountStatus ToStatus,
    string Reason,
    DateTime? Until,
    Guid? ChangedBy,
    string? ChangedByName,
    DateTime ChangedAt);

/// <summary>
/// R39: the active sessions of a user as the admin's sessions tab shows them. The parsed browser, OS and device live on
/// the login-log row of the session (R22) and are shown by the login-log page, not here; a session carries the raw user
/// agent it was opened with.
/// </summary>
public sealed record UserSessionItem(
    Guid Id,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    DateTime ExpiresAt,
    string? IpAddress,
    string? UserAgent);
