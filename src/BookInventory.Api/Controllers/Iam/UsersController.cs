using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Passwords;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R2-R6, R28-R41, R57, R58: everything a Super Admin does with the accounts of <c>iam</c>. The whole controller is
/// Super Admin only (the role claim of R44), every action binds one request, calls one use case and answers - no rule and
/// no query lives here.
/// <list type="bullet">
/// <item>Reads (list, detail, status history, sessions) answer 404 for an unknown id <em>and</em> for the hidden system
/// account (R43).</item>
/// <item>Mutations answer 204, except <c>POST users</c>, which answers 201 with the temporary password shown once (R57).</item>
/// <item>The reason of a transition is mandatory for reject, deactivate and suspend and optional for approve and
/// activate; whether it was given is the aggregate's rule, so the answer carries a code and not a text (R55).</item>
/// </list>
/// </summary>
[Route("users")]
[Authorize(Roles = "SUPER_ADMIN")]
public sealed class UsersController(
    UserQueryService queries,
    UserAccountService accounts,
    UserLifecycleService lifecycle,
    UserAccessService access,
    UserResetLinkService resetLinks,
    TemporaryPasswordResetService passwordResets) : ApiControllerBase
{
    /// <summary>R38/R43: the paged user list, with search, role / status / lockout filters and a whitelisted sort.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] UserSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R39/R43: the detail of one user: profile, status with reason and until, and the lockout state.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R33: the append-only status timeline, newest first. Rows with no actor are SYSTEM (R32) or the hidden account (R48).</summary>
    [HttpGet("{id:guid}/status-history")]
    public async Task<IActionResult> StatusHistory(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetStatusHistoryAsync(id, cancellationToken));

    /// <summary>R39: the sessions of this user that are still usable.</summary>
    [HttpGet("{id:guid}/sessions")]
    public async Task<IActionResult> Sessions(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetSessionsAsync(id, cancellationToken));

    /// <summary>R2/R57: creates a staff account and shows the temporary password once.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken cancellationToken)
    {
        var created = await accounts.CreateStaffAsync(request, cancellationToken);
        Response.Headers.CacheControl = "no-store";   // the answer carries the temporary password, shown once (R57)
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R35: full name, e-mail and mobile of somebody else's account. Your own account is refused here (R41).</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        await accounts.UpdateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R28/R57: approves a pending agent application; the note is optional.</summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] StatusChangeRequest? request, CancellationToken cancellationToken)
    {
        await lifecycle.ApproveAsync(id, request ?? new StatusChangeRequest(), cancellationToken);
        return NoContent();
    }

    /// <summary>R28: brings an inactive or suspended account back to ACTIVE.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] StatusChangeRequest? request, CancellationToken cancellationToken)
    {
        await lifecycle.ActivateAsync(id, request ?? new StatusChangeRequest(), cancellationToken);
        return NoContent();
    }

    /// <summary>R28/R37: retires an account. The reason is mandatory and every session of the user ends.</summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R30: suspends with a mandatory reason and an optional end date, after which the account lifts itself.</summary>
    [HttpPost("{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, [FromBody] StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.SuspendAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R28/R58: refuses a pending agent application; the e-mail and mobile stay reserved until released.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.RejectAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R6: moves a user between the staff roles, with a mandatory reason. The sessions end.</summary>
    [HttpPost("{id:guid}/change-role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.ChangeRoleAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R34: clears a lockout before it expires.</summary>
    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        await access.UnlockAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R36: revokes every session of this user.</summary>
    [HttpPost("{id:guid}/force-logout")]
    public async Task<IActionResult> ForceLogout(Guid id, CancellationToken cancellationToken)
    {
        await access.ForceLogoutAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R36: the user has to choose a new password at the next login.</summary>
    [HttpPost("{id:guid}/force-password-change")]
    public async Task<IActionResult> ForcePasswordChange(Guid id, CancellationToken cancellationToken)
    {
        await access.ForcePasswordChangeAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R36: sends a password reset link to the address on file, through the core notification outbox (R56).</summary>
    [HttpPost("{id:guid}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        await resetLinks.SendAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R67: a new temporary password for an Owner or a Staff account, shown once; the account must change it at the next login.</summary>
    [HttpPost("{id:guid}/temporary-password")]
    public async Task<IActionResult> ResetToTemporaryPassword(Guid id, CancellationToken cancellationToken)
    {
        var reset = await passwordResets.ResetAsync(id, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return Ok(reset);
    }

    /// <summary>R39: ends one session of this user, from the sessions tab.</summary>
    [HttpDelete("{id:guid}/sessions/{sessionId:guid}")]
    public async Task<IActionResult> RevokeSession(Guid id, Guid sessionId, CancellationToken cancellationToken)
    {
        await access.RevokeSessionAsync(id, sessionId, cancellationToken);
        return NoContent();
    }

    /// <summary>R58: frees the e-mail and mobile of a rejected agent so a new application can use them.</summary>
    [HttpPost("{id:guid}/release-identity")]
    public async Task<IActionResult> ReleaseIdentity(Guid id, CancellationToken cancellationToken)
    {
        await accounts.ReleaseIdentityAsync(id, cancellationToken);
        return NoContent();
    }
}
