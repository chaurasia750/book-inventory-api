using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Iam;
using BookInventory.Application.Iam.Accounts;
using BookInventory.Application.Iam.Staff;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R65-R67: an Owner creates and manages the Staff of their own company. The controller only binds, calls one use case
/// and answers; the company comes from the Owner's session, and a Staff account of another company, an Owner or a Super
/// Admin answers 404 like an unknown id.
/// </summary>
[Route("staff")]
[Authorize(Roles = "OWNER")]
public sealed class StaffController(
    StaffCreationService creation,
    StaffQueryService queries,
    StaffManagementService management) : ApiControllerBase
{
    /// <summary>R65/R57: creates a Staff login; the user id and the temporary password are shown once.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddStaffRequest request, CancellationToken cancellationToken)
    {
        var created = await creation.CreateAsync(request, cancellationToken);
        Response.Headers.CacheControl = "no-store";   // the answer carries the temporary password, shown once (R57)
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R66: the paged list of the company's Staff, searchable by name and user id.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] UserSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R66: one Staff account of the Owner's company.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R66: the sessions of this Staff account that are still usable.</summary>
    [HttpGet("{id:guid}/sessions")]
    public async Task<IActionResult> Sessions(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetSessionsAsync(id, cancellationToken));

    /// <summary>R66/R30: retires a Staff account. The reason is mandatory and every session of it ends.</summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] StatusChangeRequest request, CancellationToken cancellationToken)
    {
        await management.DeactivateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R66: brings an inactive Staff account back to ACTIVE.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] StatusChangeRequest? request, CancellationToken cancellationToken)
    {
        await management.ActivateAsync(id, request ?? new StatusChangeRequest(), cancellationToken);
        return NoContent();
    }

    /// <summary>R66/R34: clears a lockout before it expires.</summary>
    [HttpPost("{id:guid}/unlock")]
    public async Task<IActionResult> Unlock(Guid id, CancellationToken cancellationToken)
    {
        await management.UnlockAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R66/R36: revokes every session of this Staff account.</summary>
    [HttpPost("{id:guid}/force-logout")]
    public async Task<IActionResult> ForceLogout(Guid id, CancellationToken cancellationToken)
    {
        await management.ForceLogoutAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R67: a new temporary password, shown once; the Staff account must change it at the next login.</summary>
    [HttpPost("{id:guid}/temporary-password")]
    public async Task<IActionResult> ResetPassword(Guid id, CancellationToken cancellationToken)
    {
        var reset = await management.ResetPasswordAsync(id, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return Ok(reset);
    }
}
