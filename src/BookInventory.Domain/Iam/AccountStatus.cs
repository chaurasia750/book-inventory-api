namespace BookInventory.Domain.Iam;

/// <summary>
/// The account lifecycle status (spec R3, R28). Only <see cref="Active"/> may log in; lockout is <em>not</em> a status
/// but an automatic short block tracked by <c>locked_until</c>.
/// </summary>
public enum AccountStatus
{
    /// <summary>Agent registered, awaiting approval by a Super Admin.</summary>
    Pending,

    /// <summary>Normal working account. The only status that can log in.</summary>
    Active,

    /// <summary>Deactivated on purpose (left the company, dormant). Reactivatable.</summary>
    Inactive,

    /// <summary>Temporarily blocked with a reason and an optional end date, after which it lifts by itself.</summary>
    Suspended,

    /// <summary>Agent application refused. Final: a Super Admin releases the email/mobile (R58).</summary>
    Rejected
}
