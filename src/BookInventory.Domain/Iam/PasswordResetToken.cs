using BookInventory.BuildingBlocks.Domain;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Iam;

/// <summary>
/// A single-use password reset token (R17). It is its own root because the reset link carries the token, not the user id.
/// Only the hash is stored, the token lives for 30 minutes, and a new request invalidates earlier unused tokens of the
/// same user (the caller retires them). Resetting the password marks this one used (R18).
/// </summary>
public class PasswordResetToken : AggregateRoot
{
    private PasswordResetToken() => TokenHash = string.Empty;

    public Guid UserId { get; private set; }

    /// <summary>SHA-256 of the 256-bit random token that was emailed. Unique across all tokens.</summary>
    public string TokenHash { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public bool IsUsed => UsedAt is not null;

    public static PasswordResetToken Issue(Guid userId, string tokenHash, DateTime expiresAt) =>
        new()
        {
            UserId = userId,
            TokenHash = TextRules.RequireHash(tokenHash, IamMessages.TokenNotFound),
            ExpiresAt = expiresAt,
            UsedAt = null
        };

    /// <summary>R17/R18: checks the token is still usable. A used token is reported separately from an expired one.</summary>
    public void EnsureUsableAt(DateTime utcNow)
    {
        if (IsUsed)
            throw new ValidationFailureException(IamMessages.TokenAlreadyUsed);
        if (ExpiresAt <= utcNow)
            throw new ValidationFailureException(IamMessages.TokenExpired);
    }

    /// <summary>R18: consumes the token. One use only, enforced here.</summary>
    public void MarkUsed(DateTime utcNow)
    {
        if (IsUsed)
            throw new ValidationFailureException(IamMessages.TokenAlreadyUsed);

        UsedAt = utcNow;
    }

    /// <summary>R17: a newer request makes an earlier unused token worthless without deleting the row.</summary>
    public void Invalidate(DateTime utcNow)
    {
        if (!IsUsed)
            UsedAt = utcNow;
    }
}
