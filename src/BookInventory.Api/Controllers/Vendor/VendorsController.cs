using BookInventory.Application.Vendor;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Vendor;

/// <summary>
/// Spec vendor R1-R10: the vendor endpoints. The controller only binds, calls one use case and answers. Every route is for the
/// Owner and a Super Admin who entered the company; Staff have no access (R8). The company is never a request field: the
/// platform takes it from the session (P19, P20) and a write without one is refused there with <c>core.company_required</c>.
/// </summary>
[Route("vendors")]
[Authorize(Roles = "OWNER,SUPER_ADMIN")]
public sealed class VendorsController(
    VendorRegistrationService registration,
    VendorLifecycleService lifecycle,
    VendorQueryService queries) : ApiControllerBase
{
    /// <summary>R2-R7, R11: creates a vendor; the answer carries the generated code.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorRequest request, CancellationToken cancellationToken)
    {
        var created = await registration.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R10: the paged list with search, filters and a whitelisted sort; the account number is masked.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] VendorSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R5: one vendor with its bank details in full.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R2-R6: edits the master data.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditVendorRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the vendor off; its data stays.</summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the vendor on again.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
