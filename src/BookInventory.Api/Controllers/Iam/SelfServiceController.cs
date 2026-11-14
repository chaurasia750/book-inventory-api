using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Iam.History;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R15, R26, R35, R41: what any signed-in user does with their own account - list and end their own sessions, read their own
/// login history and edit their own profile. Every action takes the user from the validated token, never from the request,
/// so nobody can reach anybody else's data here (R27); the controller only binds, calls one use case and answers.
/// </summary>
[Route("auth")]
public sealed class SelfServiceController(
    OwnSessionService sessions,
    LoginHistoryService history,
    OwnProfileService profile) : ApiControllerBase
{
    /// <summary>R15: the caller's active sessions with their device; the one making this call is marked.</summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> Sessions(CancellationToken cancellationToken) =>
        Ok(await sessions.ListAsync(cancellationToken));

    /// <summary>R15: signs out one of the caller's own sessions from another device.</summary>
    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> RevokeSession(Guid id, CancellationToken cancellationToken)
    {
        await sessions.RevokeAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R26: the caller's own login rows, newest first, paged.</summary>
    [HttpGet("login-history")]
    public async Task<IActionResult> LoginHistory([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await history.OwnAsync(pageIndex, pageSize, cancellationToken));

    /// <summary>R41/R35: full name; an e-mail or mobile change also needs <c>currentPassword</c>.</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateOwnProfileRequest request, CancellationToken cancellationToken)
    {
        await profile.UpdateAsync(request, cancellationToken);
        return NoContent();
    }
}
