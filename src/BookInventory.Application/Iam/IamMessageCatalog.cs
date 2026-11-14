using BookInventory.BuildingBlocks.Application.Messages;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam;

/// <summary>
/// The human text of every <see cref="IamMessages"/> code (R55). This is the only place in the backend where a
/// user-facing <c>iam</c> string is written; the code travels, the text is looked up. An unknown code falls back to a
/// generic message in the API layer, so a missing entry can never leak a code to a user.
/// </summary>
public static class IamMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Account identity and profile
            [IamMessages.UserNotFound] = "The user could not be found.",
            [IamMessages.UserInvalidUserName] = "The user name must be 3 to 32 characters and may contain letters, digits, dot, underscore or hyphen.",
            [IamMessages.UserInvalidEmail] = "The email address is not valid.",
            [IamMessages.UserInvalidMobile] = "The mobile number must be 10 to 15 digits.",
            [IamMessages.UserInvalidFullName] = "The full name is required and must be at most 200 characters.",
            [IamMessages.UserDuplicateUserName] = "That user name is already taken.",
            [IamMessages.UserDuplicateEmail] = "That email address is already registered.",
            [IamMessages.UserDuplicateMobile] = "That mobile number is already registered.",
            [IamMessages.UserReservedUserName] = "That user name cannot be used.",
            [IamMessages.UserEmailRequired] = "An email address is required.",
            [IamMessages.UserEmailMissing] = "This user has no email address, so no link can be sent.",
            [IamMessages.UserMobileRequired] = "A mobile number is required.",

            // Status lifecycle
            [IamMessages.UserInvalidTransition] = "The account is not in a state that allows this change.",
            [IamMessages.UserSelfStatusChange] = "You cannot change the status of your own account.",
            [IamMessages.UserSelfActionForbidden] = "You cannot perform this action on your own account.",
            [IamMessages.UserLastSuperAdmin] = "This is the last active Super Admin, so the account must stay active.",
            [IamMessages.UserReasonRequired] = "A reason of 5 to 500 characters is required.",
            [IamMessages.UserSuspensionEndInPast] = "The end of a suspension must be in the future.",
            [IamMessages.UserResetLinkNotAllowed] = "A password reset link can only be sent to an active user.",

            // Roles
            [IamMessages.UserSelfRoleChange] = "You cannot change your own role.",
            [IamMessages.UserRoleRequired] = "A role is required.",
            [IamMessages.UserRoleNotStaff] = "That role is not a staff role.",
            [IamMessages.UserMemberRoleImmutable] = "The role of an agent cannot be changed.",
            [IamMessages.UserCompanyRoleImmutable] = "The role of a company account cannot be changed.",
            [IamMessages.UserCompanyInvalid] = "A company account needs a valid company.",
            [IamMessages.UserRoleUnchanged] = "The user already has that role.",
            [IamMessages.UserRoleChangeReasonRequired] = "A reason of 5 to 500 characters is required.",

            // System account (hidden)
            [IamMessages.SystemAccountImmutable] = "The system account cannot be modified.",

            // Registration
            [IamMessages.RegistrationPendingApproval] = "Thank you for registering. Your application is being reviewed; please contact the admin team.",

            // Lockout and credentials
            [IamMessages.UserAccountLocked] = "Your account is temporarily locked. Please try again later.",
            [IamMessages.UserNotActive] = "Your account is not active. Please contact support.",
            [IamMessages.UserPendingApproval] = "Your registration is awaiting approval. Please contact the admin team.",
            [IamMessages.UserAccountInactive] = "Your account is not active. Please contact support.",
            [IamMessages.AuthCompanyInactive] = "Your company is not active. Please contact support.",
            [IamMessages.UserAccountSuspended] = "Your account is suspended. Please contact support.",
            [IamMessages.UserApplicationRejected] = "Your registration was not accepted. Please contact support.",
            [IamMessages.UserInvalidCredentials] = "Invalid credentials.",
            [IamMessages.UserPasswordHashMissing] = "The password could not be stored.",
            [IamMessages.PasswordCurrentIncorrect] = "The current password is not correct.",
            [IamMessages.UserPasswordChangeRequired] = "You must change your password before you can continue.",
            [IamMessages.AuthRateLimited] = "Too many attempts from this address. Please wait a minute and try again.",

            // Password policy (R16), one message per missing rule. The counts live in `Auth:Password:*` (R54) and the
            // catalogue is static, so the wording names the rule instead of a number that could drift.
            [IamPasswordCodes.TooShort] = "The password does not meet the minimum length required.",
            [IamPasswordCodes.MissingDigit] = "The password does not contain enough digits.",
            [IamPasswordCodes.MissingLowerCase] = "The password does not contain enough lower case letters.",
            [IamPasswordCodes.MissingUpperCase] = "The password does not contain enough upper case letters.",
            [IamPasswordCodes.SameAsCurrent] = "The new password must be different from the current password.",

            // Sessions
            [IamMessages.SessionNotFound] = "The session could not be found.",
            [IamMessages.SessionExpired] = "The session has expired. Please sign in again.",
            [IamMessages.SessionRevoked] = "The session has ended. Please sign in again.",
            [IamMessages.SessionRefreshReuse] = "The session has ended. Please sign in again.",

            // Password reset tokens
            [IamMessages.TokenNotFound] = "The password reset link is not valid.",
            [IamMessages.TokenExpired] = "The password reset link has expired. Please request a new one.",
            [IamMessages.TokenAlreadyUsed] = "The password reset link has already been used. Please request a new one.",

            // Login log
            [IamMessages.LoginLogAlreadyClosed] = "The login record for this session was already closed.",
            [IamMessages.LoginLogExportTooLarge] = "Too many rows match. Narrow the filters (dates, user, result) and export again.",
            [IamMessages.LoginLogInvalidIp] = "The IP address filter is not a valid IPv4 or IPv6 address."
        };
}
