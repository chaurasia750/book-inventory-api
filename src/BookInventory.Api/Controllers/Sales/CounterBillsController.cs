using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R42-R49: the counter bill endpoints. The Owner, Staff and an entered Super Admin make bills and read them; only the
/// Owner and an entered Super Admin cancel (R48). The company is never a request field: the platform takes it from the session.
/// </summary>
[Route("counter-bills")]
public sealed class CounterBillsController(CounterBillService bills, CounterBillQueryService queries) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R42-R46: makes a bill, takes the stock out and answers with the number, the total and the items at or below zero stock.</summary>
    [HttpPost, Authorize(Roles = Readers)]
    public async Task<IActionResult> Make([FromBody] MakeCounterBillRequest request, CancellationToken cancellationToken)
    {
        var made = await bills.MakeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = made.CounterBillId }, made);
    }

    /// <summary>R49: the paged list with search, filters and a whitelisted sort; <c>studentId</c> lists all bills of one student.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] CounterBillSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R49: one bill with its student, lines, totals and payments.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R47: cancels a bill with a reason; the stock is given back, no ledger is touched.</summary>
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelCounterBillRequest request, CancellationToken cancellationToken)
    {
        await bills.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
