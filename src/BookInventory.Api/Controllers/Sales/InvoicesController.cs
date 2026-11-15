using BookInventory.Application.Sales;
using BookInventory.Domain.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R8-R15: invoices. The Owner, Staff and an entered Super Admin make an invoice from a confirmed order and read
/// invoices; only the Owner and an entered Super Admin cancel one.
/// </summary>
public sealed class InvoicesController(InvoiceService invoices, InvoiceQueryService queries) : ApiControllerBase
{
    private const string Sellers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R8-R11: the invoice of a confirmed order; the answer lists the items whose Main stock went to zero or below.</summary>
    [HttpPost("school-orders/{orderId:guid}/invoices"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Make(Guid orderId, [FromBody] MakeInvoiceRequest request, CancellationToken cancellationToken)
    {
        var made = await invoices.MakeAsync(orderId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = made.InvoiceId }, made);
    }

    [HttpGet("invoices"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> List([FromQuery] InvoiceQuery query, CancellationToken cancellationToken) => Ok(await queries.ListAsync(query, cancellationToken));

    /// <summary>R13, R26: one invoice; <c>layout</c> shows its lines by class or as a book list (the school default when empty).</summary>
    [HttpGet("invoices/{id:guid}"), Authorize(Roles = Sellers)]
    public async Task<IActionResult> Get(Guid id, [FromQuery] InvoiceLayout? layout, CancellationToken cancellationToken) => Ok(await queries.GetAsync(id, layout, cancellationToken));

    /// <summary>R12: the Owner cancels with a reason; stock and ledger are reversed.</summary>
    [HttpPost("invoices/{id:guid}/cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelInvoiceRequest request, CancellationToken cancellationToken)
    {
        await invoices.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
