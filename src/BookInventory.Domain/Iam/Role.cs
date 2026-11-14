namespace BookInventory.Domain.Iam;

/// <summary>
/// The four fixed roles of the system (spec R1). Role is a column, not a permission table set; it is stored as
/// UPPER_SNAKE text (<c>SUPER_ADMIN</c>) by the platform convention.
/// </summary>
public enum Role
{
    /// <summary>Full administrative power, including money and approvals. The only role allowed to manage accounts.</summary>
    SuperAdmin,

    /// <summary>Internal sales/CRM staff. Created by a Super Admin, never self-registered.</summary>
    SalesCrm,

    /// <summary>Internal accounts staff (payouts, commission verification).</summary>
    Accounts,

    /// <summary>An agent. Log in with the generated user id, never with email or mobile (R7).</summary>
    Member
}

public static class RoleExtensions
{
    /// <summary>The staff roles a Super Admin may switch a user between (R6). <c>MEMBER</c> is never one of them.</summary>
    public static bool IsStaff(this Role role) => role is Role.SuperAdmin or Role.SalesCrm or Role.Accounts;
}
