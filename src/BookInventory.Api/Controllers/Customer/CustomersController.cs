using BookInventory.Application.Customer;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Customer;

/// <summary>
/// Spec customer R1-R11: the customer endpoints. The controller only binds, calls one use case and answers. The Owner and an
/// entered Super Admin do everything; Staff read, suggest and create but never edit or switch a customer (R9). The company is
/// never a request field: the platform takes it from the session (P19, P20) and a write without one is refused there with
/// <c>core.company_required</c>.
/// </summary>
[Route("customers")]
public sealed class CustomersController(
    CustomerRegistrationService registration,
    CustomerLifecycleService lifecycle,
    CustomerQueryService queries) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Managers = "OWNER,SUPER_ADMIN";

    /// <summary>R2-R7, R10: creates a customer; the answer carries the generated code.</summary>
    [HttpPost, Authorize(Roles = Readers)]
    public async Task<IActionResult> Create([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var created = await registration.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R11: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] CustomerSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R8: at most 20 active customers whose name or mobile contains the text.</summary>
    [HttpGet("suggest"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Suggest([FromQuery] string? text, CancellationToken cancellationToken) =>
        Ok(await queries.SuggestAsync(text, cancellationToken));

    /// <summary>One customer.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R2-R7: edits the master data.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = Managers)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCustomerRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R5: switches the customer off; its data stays.</summary>
    [HttpPost("{id:guid}/deactivate"), Authorize(Roles = Managers)]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R5: switches the customer on again.</summary>
    [HttpPost("{id:guid}/activate"), Authorize(Roles = Managers)]
    public async Task<IActionResult> Activate(Guid id, CancellationToken cancellationToken)
    {
        await lifecycle.ReactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
