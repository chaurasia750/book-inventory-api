namespace BookInventory.Domain.Iam;

/// <summary>
/// Every message code the <c>iam</c> module can return (spec R55). The domain throws these codes; the human text
/// lives in the message catalogue (<c>BookInventory.Application.Iam.IamMessageCatalog</c>), so no user-facing string is
/// hard-coded here. Codes are stable API surface - never reword one, add a new one.
/// </summary>
public static class IamMessages
{
    // Account identity and profile
    public const string UserNotFound = "iam.user.not_found";
    public const string UserInvalidUserName = "iam.user.invalid_username";
    public const string UserInvalidEmail = "iam.user.invalid_email";
    public const string UserInvalidMobile = "iam.user.invalid_mobile";
    public const string UserInvalidFullName = "iam.user.invalid_full_name";
    public const string UserDuplicateUserName = "iam.user.duplicate_username";
    public const string UserDuplicateEmail = "iam.user.duplicate_email";
    public const string UserDuplicateMobile = "iam.user.duplicate_mobile";
    public const string UserReservedUserName = "iam.user.reserved_username";
    public const string UserEmailRequired = "iam.user.email_required";
    public const string UserEmailMissing = "iam.user.email_missing";
    public const string UserMobileRequired = "iam.user.mobile_required";

    // Status lifecycle
    public const string UserInvalidTransition = "iam.user.invalid_transition";
    public const string UserSelfStatusChange = "iam.user.self_status_change";
    public const string UserSelfActionForbidden = "iam.user.self_action_forbidden";
    public const string UserLastSuperAdmin = "iam.user.last_super_admin";
    public const string UserReasonRequired = "iam.user.reason_required";
    public const string UserSuspensionEndInPast = "iam.user.suspension_end_in_past";

    /// <summary>R36: a reset link only makes sense for an account that may log in.</summary>
    public const string UserResetLinkNotAllowed = "iam.user.reset_link_not_allowed";

    // Roles
    public const string UserSelfRoleChange = "iam.user.self_role_change";
    public const string UserRoleRequired = "iam.user.role_required";
    public const string UserRoleNotStaff = "iam.user.role_not_staff";
    public const string UserMemberRoleImmutable = "iam.user.member_role_immutable";
    public const string UserCompanyRoleImmutable = "iam.user.company_role_immutable";
    public const string UserCompanyInvalid = "iam.user.company_invalid";
    public const string StaffShortNameInvalid = "iam.staff.invalid_short_name";
    public const string UserPasswordResetNotAllowed = "iam.user.password_reset_not_allowed";
    public const string UserRoleUnchanged = "iam.user.role_unchanged";
    public const string UserRoleChangeReasonRequired = "iam.user.role_change_reason_required";

    /// <summary>R53/R55: the welcome text shown after an agent registers under <c>Registration:Mode = ApprovalRequired</c>.</summary>
    public const string RegistrationPendingApproval = "iam.registration.pending_approval";

    // System account (hidden)
    public const string SystemAccountImmutable = "iam.system_account.immutable";

    // Lockout and credentials
    public const string UserAccountLocked = "iam.auth.account_locked";
    public const string UserNotActive = "iam.auth.not_active";
    public const string UserPendingApproval = "iam.auth.pending_approval";
    public const string UserAccountInactive = "iam.auth.account_inactive";
    public const string AuthCompanyInactive = "iam.auth.company_inactive";
    public const string UserAccountSuspended = "iam.auth.account_suspended";
    public const string UserApplicationRejected = "iam.auth.application_rejected";
    public const string UserInvalidCredentials = "iam.auth.invalid_credentials";
    public const string UserPasswordHashMissing = "iam.password.hash_missing";

    /// <summary>R19: change-password needs the current password and it was wrong. A 400, not a 401: the caller is signed in.</summary>
    public const string PasswordCurrentIncorrect = "iam.password.current_incorrect";

    /// <summary>R12: the forced password change blocks every endpoint except change-password, me and logout.</summary>
    public const string UserPasswordChangeRequired = "iam.auth.password_change_required";

    /// <summary>R10: too many login attempts from one client IP inside the window.</summary>
    public const string AuthRateLimited = "iam.auth.rate_limited";

    // Sessions
    public const string SessionNotFound = "iam.session.not_found";
    public const string SessionExpired = "iam.session.expired";
    public const string SessionRevoked = "iam.session.revoked";
    public const string SessionRefreshReuse = "iam.session.refresh_reuse";

    // Password reset tokens
    public const string TokenNotFound = "iam.token.not_found";
    public const string TokenExpired = "iam.token.expired";
    public const string TokenAlreadyUsed = "iam.token.already_used";

    // Login log
    public const string LoginLogAlreadyClosed = "iam.login_log.already_closed";

    /// <summary>R26: an export would exceed the row cap; the caller narrows the filters.</summary>
    public const string LoginLogExportTooLarge = "iam.login_log.export_too_large";

    /// <summary>R26: the IP filter is not an IPv4 or IPv6 address.</summary>
    public const string LoginLogInvalidIp = "iam.login_log.invalid_ip";
}
