using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R64-R69: the school refund endpoints. Only the Owner and an entered Super Admin record and cancel a refund (R67); Staff
/// may read. The company is never a request field: the platform takes it from the session.
/// </summary>
[Route("refunds")]
public sealed class RefundsController(RefundService refunds, RefundQueryService queries, RefundSlipService slips) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R64-R66: records a refund of at most the school's credit and posts the ledger debit.</summary>
    [HttpPost, Authorize(Roles = Owners)]
    public async Task<IActionResult> Record([FromBody] RecordRefundRequest request, CancellationToken cancellationToken)
    {
        var made = await refunds.RecordAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = made.RefundId }, made);
    }

    /// <summary>R69: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] RefundSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R69: one refund.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R68: the refund slip as an A5 PDF, rendered on demand and never stored.</summary>
    [HttpGet("{id:guid}/pdf"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var pdf = await slips.RenderAsync(id, cancellationToken);
        Response.Headers.ContentDisposition = $"inline; filename=\"{pdf.FileName}\"";
        return File(pdf.Content, "application/pdf");
    }

    /// <summary>R67: cancels a refund with a reason; the ledger posting is reversed.</summary>
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelRefundRequest request, CancellationToken cancellationToken)
    {
        await refunds.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
