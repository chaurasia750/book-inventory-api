using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R38, R39, R48, R49: the counter endpoints. The Owner and an entered Super Admin open, edit, close and reopen counters;
/// Staff read them. The company is never a request field: the platform takes it from the session (P19, P20).
/// </summary>
[Route("counters")]
public sealed class CountersController(CounterService counters, CounterQueryService queries) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R38: opens a counter for a school that sells at the counter.</summary>
    [HttpPost, Authorize(Roles = Owners)]
    public async Task<IActionResult> Open([FromBody] SaveCounterRequest request, CancellationToken cancellationToken)
    {
        var opened = await counters.OpenAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = opened.Id }, opened);
    }

    /// <summary>R38: changes the name, session and dates; the school only while no bill exists.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Edit(Guid id, [FromBody] SaveCounterRequest request, CancellationToken cancellationToken)
    {
        await counters.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R39: a closed counter takes no new bill.</summary>
    [HttpPost("{id:guid}/close"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Close(Guid id, CancellationToken cancellationToken)
    {
        await counters.CloseAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R39: opens a closed counter again.</summary>
    [HttpPost("{id:guid}/open"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Reopen(Guid id, CancellationToken cancellationToken)
    {
        await counters.ReopenAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R49: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] CounterSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R49: one counter.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));
}
