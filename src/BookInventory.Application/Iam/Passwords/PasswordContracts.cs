using System.ComponentModel.DataAnnotations;
using BookInventory.Domain.Iam;

namespace BookInventory.Application.Iam.Passwords;

/// <summary>R19: the current password proves the caller, the new one must differ and meet the policy (R16). Bounds are data annotations so the API answers 400 before a use case sees an empty or oversized value.</summary>
public sealed record ChangePasswordRequest(
    [param: Required, StringLength(256, MinimumLength = 1)] string CurrentPassword,
    [param: Required, StringLength(256, MinimumLength = 1)] string NewPassword);

/// <summary>R17: the address the reset link is asked for. The answer never says whether it exists.</summary>
public sealed record ForgotPasswordRequest([param: Required, StringLength(254, MinimumLength = 3)] string Email);

/// <summary>R18: the token from the mailed link and the new password.</summary>
public sealed record ResetPasswordRequest(
    [param: Required, StringLength(512, MinimumLength = 1)] string Token,
    [param: Required, StringLength(256, MinimumLength = 1)] string NewPassword);

/// <summary>Whether a reset link can still be used, for the page that opens it (validity only, never why).</summary>
public sealed record VerifyResetTokenResponse(bool Valid);

/// <summary>
/// One password change, whoever asks for it (R18, R19): the account, the new password, why the user's sessions end and
/// which session survives - the caller's own on a change, none on a reset.
/// </summary>
public sealed record PasswordChange(AppUser User, string NewPassword, LogoutReason Reason, Guid? KeepSessionId = null);
