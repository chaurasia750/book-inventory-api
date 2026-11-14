using BookInventory.Api.Auth;
using BookInventory.Api.Hosting;
using BookInventory.Application.Iam.Passwords;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R16-R19: the password endpoints. The controller only binds, calls one use case and answers; the policy, the
/// revocation of sessions and the mail live in the use cases.
/// <list type="bullet">
/// <item><c>change-password</c> needs a signed-in caller and is one of the three endpoints a forced change still allows
/// (R12); it answers with a fresh access token, because the old one still says the change is pending.</item>
/// <item><c>forgot-password</c> answers 202 with an empty body for every address (R17) and is rate limited per client IP (R10).</item>
/// <item><c>reset-password</c> and its <c>verify</c> step are anonymous: the emailed token is the credential.</item>
/// </list>
/// </summary>
[Route("auth")]
public sealed class PasswordController(
    ChangePasswordService changePassword,
    ForgotPasswordService forgotPassword,
    ResetPasswordService resetPassword,
    SessionCookieWriter cookies) : ApiControllerBase
{
    /// <summary>R19: current password, new password. Other sessions end; this one continues with a new access token.</summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var response = await changePassword.ChangeAsync(request, cancellationToken);
        cookies.WriteAccess(Response, response);
        return Ok(response);
    }

    /// <summary>R17: always 202, whatever the address.</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthRateLimiting.ForgotPasswordPolicyName)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await forgotPassword.RequestAsync(request, cancellationToken);
        return Accepted();
    }

    /// <summary>R17/R18: whether the emailed link can still be used (validity only, never why not).</summary>
    [HttpGet("reset-password/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string? token, CancellationToken cancellationToken) =>
        Ok(await resetPassword.VerifyAsync(token, cancellationToken));

    /// <summary>R18: sets the new password; every session of the user ends.</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await resetPassword.ResetAsync(request, cancellationToken);
        return NoContent();
    }
}
