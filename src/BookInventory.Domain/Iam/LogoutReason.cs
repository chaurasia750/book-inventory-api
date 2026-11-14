namespace BookInventory.Domain.Iam;

/// <summary>
/// Why a session ended, and the value stored in <c>user_login_log.logout_reason</c> (R24). The same vocabulary is used
/// by <see cref="UserSession"/> and <see cref="UserLoginLog"/>, so a session and the login row that recorded it always
/// agree on why it ended.
/// </summary>
public enum LogoutReason
{
    /// <summary>The user signed out of this session (R14).</summary>
    UserLogout,

    /// <summary>The user signed out everywhere (R14).</summary>
    LogoutAll,

    /// <summary>The session simply ran out; <c>logged_out_at = expires_at</c>.</summary>
    Expired,

    /// <summary>A Super Admin revoked this one session (R36 force logout).</summary>
    RevokedByAdmin,

    /// <summary>The user changed their password; every other session died (R19).</summary>
    PasswordChanged,

    /// <summary>The password was reset by email; every session died (R18).</summary>
    PasswordReset,

    /// <summary>The account was deactivated (R31).</summary>
    AccountDeactivated,

    /// <summary>The account was suspended (R31).</summary>
    AccountSuspended,

    /// <summary>A Super Admin forced all sessions to end (R36).</summary>
    ForcedLogout,

    /// <summary>An already used refresh token was presented again (R13 reuse detection).</summary>
    RefreshReuse,

    /// <summary>The user ended one of their own other sessions from the sessions screen (R15).</summary>
    RevokedByUser
}
