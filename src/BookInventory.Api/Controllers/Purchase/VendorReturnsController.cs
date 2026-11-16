using BookInventory.Application.Purchase;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Purchase;

/// <summary>
/// Spec purchase R2-R12: the vendor return endpoints. The Owner, Staff and an entered Super Admin make vendor returns and read them;
/// only the Owner and an entered Super Admin cancel (R9). The company is never a request field: the platform takes it from the session.
/// </summary>
[Route("vendor-returns")]
public sealed class VendorReturnsController(VendorReturnService returns, VendorReturnQueryService queries, DebitNotePdfService pdfs) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R2-R7: makes a vendor return, takes the books out of stock, debits the vendor and answers with the number, the total and the items at or below zero stock.</summary>
    [HttpPost, Authorize(Roles = Readers)]
    public async Task<IActionResult> Make([FromBody] MakeVendorReturnRequest request, CancellationToken cancellationToken)
    {
        var made = await returns.MakeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = made.VendorReturnId }, made);
    }

    /// <summary>R12: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] VendorReturnSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R12: one vendor return with its lines and totals.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R11: the vendor return as an A4 Debit Note PDF, rendered on demand and never stored.</summary>
    [HttpGet("{id:guid}/pdf"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Pdf(Guid id, CancellationToken cancellationToken)
    {
        var pdf = await pdfs.RenderAsync(id, cancellationToken);
        Response.Headers.ContentDisposition = $"inline; filename=\"{pdf.FileName}\"";
        return File(pdf.Content, "application/pdf");
    }

    /// <summary>R8: cancels a vendor return with a reason; the stock and the ledger are reversed.</summary>
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelVendorReturnRequest request, CancellationToken cancellationToken)
    {
        await returns.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
