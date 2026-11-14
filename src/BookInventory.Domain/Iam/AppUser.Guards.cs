using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

public partial class AppUser
{
    // ---------------------------------------------------------------- guards and normalisation

    /// <summary>Who changed the status and when; a <see cref="Guid.Empty"/> actor is SYSTEM and is stored as null.</summary>
    private readonly record struct ChangeStamp(Guid ActorId, DateTime UtcNow);

    private void SetStatus(AccountStatus newStatus, string? reason, DateTime? suspendedUntil, ChangeStamp stamp)
    {
        Status = newStatus;
        StatusReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        StatusChangedAt = stamp.UtcNow;
        StatusChangedBy = stamp.ActorId == Guid.Empty ? null : stamp.ActorId;
        SuspendedUntil = suspendedUntil;
    }

    private void EnsureTransition(params AccountStatus[] allowedFrom)
    {
        if (!allowedFrom.Contains(Status))
            throw new ConflictException(IamMessages.UserInvalidTransition);
    }

    /// <summary>R5: the last visible active Super Admin must always exist, or nobody could administer the system.</summary>
    private void EnsureNotLastSuperAdmin(int otherActiveVisibleSuperAdmins)
    {
        if (Role == Role.SuperAdmin && Status == AccountStatus.Active && otherActiveVisibleSuperAdmins == 0)
            throw new ConflictException(IamMessages.UserLastSuperAdmin);
    }

    private void EnsureNotSelf(Guid actorId)
    {
        if (actorId == Id)
            throw new ConflictException(IamMessages.UserSelfStatusChange);
    }

    private void EnsureNoSelfAction(Guid actorId)
    {
        if (actorId == Id)
            throw new ValidationFailureException(IamMessages.UserSelfActionForbidden);
    }

    /// <summary>R42: no endpoint, screen or event may change anything about the system account.</summary>
    private void EnsureNotSystemAccount()
    {
        if (IsSystemAccount)
            throw new ForbiddenException(IamMessages.SystemAccountImmutable);
    }

    private void EnsureSystemAccount()
    {
        if (!IsSystemAccount)
            throw new ForbiddenException(IamMessages.SystemAccountImmutable);
    }

    private static string RequireFullName(string? value)
    {
        var trimmed = value?.Trim();
        if (!IdentityNormalizer.IsValidFullName(trimmed))
            throw new ValidationFailureException(IamMessages.UserInvalidFullName);
        return trimmed;
    }

    private static string RequireUserName(string? value, bool allowReserved)
    {
        var normalized = IdentityNormalizer.NormalizeUserName(value);
        if (!IdentityNormalizer.IsValidUserName(normalized))
            throw new ValidationFailureException(IamMessages.UserInvalidUserName);
        if (!allowReserved && ReservedUserNames.IsReserved(normalized))
            throw new ValidationFailureException(IamMessages.UserReservedUserName);
        return normalized;
    }

    private static string? NormalizeEmail(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeEmail(value);
        if (normalized is null)
            return null;
        if (!IdentityNormalizer.IsValidEmail(normalized))
            throw new ValidationFailureException(IamMessages.UserInvalidEmail);
        return normalized;
    }

    private static string? NormalizeMobile(string? value)
    {
        var normalized = IdentityNormalizer.NormalizeMobile(value);
        if (normalized is null)
            return null;
        if (!IdentityNormalizer.IsValidMobile(normalized))
            throw new ValidationFailureException(IamMessages.UserInvalidMobile);
        return normalized;
    }

    private static string RequirePasswordHash(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationFailureException(IamMessages.UserPasswordHashMissing);
        return value;
    }

    private static string RequireReason(string? value, string code = IamMessages.UserReasonRequired)
    {
        var trimmed = value?.Trim();
        if (trimmed is not { Length: >= ReasonMinLength and <= ReasonMaxLength })
            throw new ValidationFailureException(code);
        return trimmed;
    }
}
