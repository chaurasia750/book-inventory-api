namespace BookInventory.Domain.Iam;

/// <summary>
/// The fixed roles of the system (spec R1, R60). Role is a column, not a permission table set; it is stored as
/// UPPER_SNAKE text (<c>SUPER_ADMIN</c>) by the platform convention.
/// </summary>
public enum Role
{
    /// <summary>Full administrative power, including money and approvals. The only role allowed to manage accounts.</summary>
    SuperAdmin,

    /// <summary>Internal sales/CRM staff. Created by a Super Admin, never self-registered.</summary>
    SalesCrm,

    /// <summary>Internal accounts staff.</summary>
    Accounts,

    /// <summary>An agent. Log in with the generated user id, never with email or mobile (R7).</summary>
    Member,

    /// <summary>The main account of a company (R60). Created together with the company; logs in with its user id (R64).</summary>
    Owner,

    /// <summary>A login the Owner creates for the company's people (R60, R65). Denied by default (R68).</summary>
    Staff
}

public static class RoleExtensions
{
    /// <summary>The staff roles a Super Admin may switch a user between (R6). <c>MEMBER</c> is never one of them.</summary>
    public static bool IsStaff(this Role role) => role is Role.SuperAdmin or Role.SalesCrm or Role.Accounts;

    /// <summary>R60: <c>OWNER</c> and <c>STAFF</c> belong to a company, are never created by staff creation and never change.</summary>
    public static bool IsCompanyRole(this Role role) => role is Role.Owner or Role.Staff;
}
