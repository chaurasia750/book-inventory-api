namespace BookInventory.Domain.Iam;

/// <summary>
/// The lockout policy of R9, passed in from configuration (R54) so the rule lives in one place and stays testable:
/// after <see cref="MaxAttempts"/> consecutive wrong passwords the account is locked for <see cref="LockoutDuration"/>.
/// </summary>
public sealed record LockoutPolicy(int MaxAttempts, TimeSpan LockoutDuration)
{
    public static LockoutPolicy Default { get; } = new(5, TimeSpan.FromMinutes(15));
}
