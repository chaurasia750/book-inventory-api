using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Company;

namespace BookInventory.Api.Controllers.Company;

/// <summary>
/// R2-R10: the company endpoints. The controller only binds, calls one use case and answers. Everything but
/// <c>companies/current</c> is for the Super Admin only; <c>current</c> answers any authenticated caller with the
/// company of its own session, never of a request field (R6, R10).
/// </summary>
[Route("companies")]
public sealed class CompaniesController(
    CompanyRegistrationService registration,
    CompanyLifecycleService lifecycle,
    CompanyQueryService queries) : ApiControllerBase
{
    /// <summary>R3/R4: creates a company with its Owner; the Owner's user id and temporary password are shown once.</summary>
    [HttpPost, Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var created = await registration.CreateAsync(request, cancellationToken);
        Response.Headers.CacheControl = "no-store";   // the answer carries the temporary password, shown once (R4)
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R9: the paged company list with search, status filter and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> List([FromQuery] CompanySearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R9/R11: one company with its status timeline and its accounts.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R3: edits the master data.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCompanyRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the company off with a reason and ends every session of its accounts.</summary>
    [HttpPost("{id:guid}/deactivate"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateCompanyRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the company on again; the note is optional.</summary>
    [HttpPost("{id:guid}/activate"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateCompanyRequest? request, CancellationToken cancellationToken)
    {
        await lifecycle.ReactivateAsync(id, request ?? new ActivateCompanyRequest(), cancellationToken);
        return NoContent();
    }

    /// <summary>R6/R10: the company of this request's session (a company account's own, or the one a Super Admin entered).</summary>
    [HttpGet("current")]
    public async Task<IActionResult> Current(CancellationToken cancellationToken) =>
        Ok(await queries.GetCurrentAsync(cancellationToken));
}
