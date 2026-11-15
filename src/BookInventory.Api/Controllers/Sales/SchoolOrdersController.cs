using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R2-R6, R14, R15: school orders. The Owner, Staff and an entered Super Admin draft, change, delete and confirm orders;
/// only the Owner and an entered Super Admin close one. The company is never a request field.
/// </summary>
[Route("school-orders")]
public sealed class SchoolOrdersController(SchoolOrderService orders, SchoolOrderQueryService queries) : ApiControllerBase
{
    private const string Sellers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    [HttpGet, Authorize(Roles = Sellers)]
    public async Task<IActionResult> List([FromQuery] SchoolOrderQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.ListAsync(query, cancellationToken));

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
