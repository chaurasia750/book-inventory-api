using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.BuildingBlocks.Configuration.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BookInventory.Api.Auth;
using BookInventory.Api.Hosting;
using BookInventory.Application.Iam.Authentication;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R11-R15/R27: the authentication endpoints. The controller only binds, calls one use case and answers; every rule
/// (identifier rules, lockout, status, token lifetimes, cookie flags) lives in the use cases.
/// <list type="bullet">
/// <item><c>login</c> and <c>refresh</c> are anonymous and both marked <c>[PersistOnFailure]</c>: their refusals are
/// returned, not thrown, and the attempt, the lockout counter and the reuse revoke have to be committed although the
/// response is 401/423 (R21, R13).</item>
/// <item>The refresh token never appears in a body - it goes into the HttpOnly cookie through
/// <see cref="SessionCookieWriter"/> together with the access cookie (R11, D2).</item>
/// </list>
/// </summary>
[Route("auth")]
public sealed class AuthController(
    LoginService login,
    RefreshService refresh,
    LogoutService logout,
    MeService me,
    SessionCookieWriter cookies,
    AuthenticationProblemFactory problems) : ApiControllerBase
{
    /// <summary>R7: identifier + password + "remember me". Agents pass their generated user id, staff their e-mail or user name.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [PersistOnFailure]
    [EnableRateLimiting(AuthRateLimiting.PolicyName)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request, ClientContextFactory.Create(HttpContext));
        return Answer(await login.LoginAsync(command, cancellationToken));
    }

    /// <summary>R13: rotates the refresh token and returns a new access token. Anonymous, because the cookie is the credential.</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [PersistOnFailure]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken) =>
        Answer(await refresh.RefreshAsync(cookies.ReadRefreshToken(Request), cancellationToken));

    /// <summary>R14: revokes the session of the access token that made this call.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await logout.LogoutAsync(cancellationToken);
        cookies.Clear(Response);
        return NoContent();
    }

    /// <summary>R14: revokes every session of the caller, including this one.</summary>
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        await logout.LogoutAllAsync(cancellationToken);
        cookies.Clear(Response);
        return NoContent();
    }

    /// <summary>R12/R27: the caller's own profile, role and forced-change flag. Allowed while a password change is forced.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken) => Ok(await me.GetMeAsync(cancellationToken));

    /// <summary>Writes the rotated cookie on success, answers the catalogue problem on refusal.</summary>
    private IActionResult Answer<TResponse>(AuthOutcome<IssuedSession<TResponse>> outcome)
        where TResponse : class, IAccessTokenResponse
    {
        if (outcome.Refused is { } refused)
            return StatusCode((int)refused.Status, problems.For(refused));

        var session = outcome.Value;
        cookies.Write(Response, session.Cookie, session.Response);
        return Ok(session.Response);
    }
}
