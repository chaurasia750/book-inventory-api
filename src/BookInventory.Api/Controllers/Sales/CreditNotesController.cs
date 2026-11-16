using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R53-R63: the credit note endpoints. The Owner, Staff and an entered Super Admin make credit notes and read them; only
/// the Owner and an entered Super Admin cancel (R60). The company is never a request field: the platform takes it from the session.
/// </summary>
[Route("credit-notes")]
public sealed class CreditNotesController(CreditNoteService notes, CreditNoteQueryService queries) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R53-R57: makes a credit note, takes the books into stock, credits the school and answers with the number, the total and the over-returned items.</summary>
    [HttpPost, Authorize(Roles = Readers)]
    public async Task<IActionResult> Make([FromBody] MakeCreditNoteRequest request, CancellationToken cancellationToken)
    {
        var made = await notes.MakeAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = made.CreditNoteId }, made);
    }

    /// <summary>R61: the paged list with search, filters and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] CreditNoteSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R61: one credit note with its lines, totals, reason and invoice.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R58: cancels a credit note with a reason; the stock and the ledger are reversed and the order takes the quantities back.</summary>
    [HttpPost("{id:guid}/cancel"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelCreditNoteRequest request, CancellationToken cancellationToken)
    {
        await notes.CancelAsync(id, request, cancellationToken);
        return NoContent();
    }
}
