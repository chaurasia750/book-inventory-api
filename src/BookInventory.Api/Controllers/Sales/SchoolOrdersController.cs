using BookInventory.Application.Sales;
using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R2-R6, R14, R15: school orders. The Owner, Staff and an entered Super Admin draft, change, delete and confirm orders;
/// only the Owner and an entered Super Admin close one. The company is never a request field.
/// </summary>
[Route("school-orders")]
public sealed class SchoolOrdersController(
    SchoolOrderService orders, SchoolOrderQueryService queries, OrderLineAdditionService additions, ISchoolSessions sessions) : ApiControllerBase
{
    private const string Sellers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    [HttpGet, Authorize(Roles = Sellers)]
    public async Task<IActionResult> List([FromQuery] SchoolOrderQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.ListAsync(query, cancellationToken));

    /// <summary>R2 (school R18): the school's session on a date and the sessions before and after it, for the order screen's choice.</summary>
    [HttpGet("session-choice"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> SessionChoice([FromQuery] Guid schoolId, [FromQuery] DateOnly date, CancellationToken cancellationToken) =>
        await sessions.SessionOfAsync(schoolId, date, cancellationToken) is { } choice ? Ok(choice) : NotFound();

    [HttpGet("{id:guid}"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => Ok(await queries.GetAsync(id, cancellationToken));

    [HttpPost, Authorize(Roles = Sellers)]
    public async Task<IActionResult> Create([FromBody] SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        var order = await orders.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = order.Id }, await queries.DetailOfAsync(order, cancellationToken));
    }

    [HttpPut("{id:guid}"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Replace(Guid id, [FromBody] SaveSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        await orders.ReplaceAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await orders.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R22: adds the books of whole classes (students x quantity per student) to a draft or confirmed order.</summary>
    [HttpPost("{id:guid}/class-sets"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> AddClassSets(Guid id, [FromBody] AddClassSetsRequest request, CancellationToken cancellationToken) =>
        Ok(await additions.AddClassSetsAsync(id, request, cancellationToken));

    /// <summary>R23: adds many chosen items with one quantity and an optional class.</summary>
    [HttpPost("{id:guid}/bulk-lines"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> AddBulkLines(Guid id, [FromBody] AddBulkLinesRequest request, CancellationToken cancellationToken) =>
        Ok(await additions.AddBulkAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/confirm"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        await orders.ConfirmAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/close"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseSchoolOrderRequest request, CancellationToken cancellationToken)
    {
        await orders.CloseAsync(id, request, cancellationToken);
        return NoContent();
    }
}
