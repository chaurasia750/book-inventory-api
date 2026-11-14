using BookInventory.BuildingBlocks.Domain;

namespace BookInventory.Domain.Iam;

/// <summary>
/// Domain events raised by <see cref="AppUser"/> (spec R40, section 6). They carry ids and values, never entities, and
/// are handled in the Application layer: <c>UserDeactivated</c> / <c>UserSuspended</c> revoke the user's sessions and
/// close their login-log rows, <c>UserActivated</c> / <c>UserRejected</c> let <c>mlm</c> react, and
/// <c>UserRoleChanged</c> / <c>PasswordChanged</c> revoke sessions so the change applies on the next login.
/// </summary>
public abstract record UserEvent(Guid UserId) : DomainEventBase;

/// <summary>The account became usable again: approved, reactivated, or a suspension that lifted.</summary>
public sealed record UserActivated(Guid UserId, AccountStatus FromStatus, bool LiftedAutomatically) : UserEvent(UserId);

/// <summary>The account was deactivated on purpose (R37: the way to retire an account; it is never deleted).</summary>
public sealed record UserDeactivated(Guid UserId, string Reason) : UserEvent(UserId);

/// <summary>The account was suspended for a reason, until <paramref name="Until"/> if that is set.</summary>
public sealed record UserSuspended(Guid UserId, string Reason, DateTime? Until) : UserEvent(UserId);

/// <summary>The agent application was refused. Final (R58).</summary>
public sealed record UserRejected(Guid UserId, string Reason) : UserEvent(UserId);

/// <summary>A staff user was moved to another staff role (R6). Sessions are revoked so the new role applies next login.</summary>
public sealed record UserRoleChanged(Guid UserId, Role FromRole, Role ToRole, string Reason) : UserEvent(UserId);

/// <summary>The password changed (R19: every *other* session of the user is revoked).</summary>
public sealed record UserPasswordChanged(Guid UserId, bool ForcedChange) : UserEvent(UserId);
