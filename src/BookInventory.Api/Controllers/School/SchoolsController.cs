using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.School;

namespace BookInventory.Api.Controllers.School;

/// <summary>
/// Spec school R1-R11: the school endpoints. The controller only binds, calls one use case and answers. Reads are open to
/// the Owner, Staff and a Super Admin who entered the company (Staff see active schools only, applied in the use case);
/// writes are for the Owner and the entered Super Admin. The company is never a request field: the platform takes it from
/// the session (P19, P20) and a write without one is refused there with <c>core.company_required</c>.
/// </summary>
[Route("schools")]
public sealed class SchoolsController(
    SchoolRegistrationService registration,
    SchoolLifecycleService lifecycle,
    SchoolQueryService queries) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R2-R8, R15: creates a school; the answer carries the generated code.</summary>
    [HttpPost, Authorize(Roles = Writers)]
    public async Task<IActionResult> Create([FromBody] CreateSchoolRequest request, CancellationToken cancellationToken)
    {
        var created = await registration.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R9, R11: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] SchoolSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R9: one school with its class list.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R2-R5, R7: edits the master data.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditSchoolRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R6: replaces the class list.</summary>
    [HttpPut("{id:guid}/classes"), Authorize(Roles = Writers)]
    public async Task<IActionResult> ReplaceClasses(Guid id, [FromBody] ReplaceClassesRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.ReplaceClassesAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R8: switches the school off; its data stays.</summary>
    [HttpPost("{id:guid}/deactivate"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R8: switches the school on again.</summary>
    [HttpPost("{id:guid}/activate"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
