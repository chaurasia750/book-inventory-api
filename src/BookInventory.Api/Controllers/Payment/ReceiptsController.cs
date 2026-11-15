using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Payment;

/// <summary>
/// Spec payment R2-R9, R16: school payment receipts. Only the Owner and a Super Admin who entered the company record and read them
/// (Staff get 403). The same <c>clientRequestId</c> answers <c>200</c> with the receipt already recorded instead of a second one (R9).
/// </summary>
[Route("receipts")]
public sealed class ReceiptsController(ReceiptService receipts, ReceiptQueryService queries) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R4-R9: records a payment, applies it to invoices and credits the school ledger.</summary>
    [HttpPost, Authorize(Roles = Owners)]
    public async Task<IActionResult> Record([FromBody] RecordReceiptRequest request, CancellationToken cancellationToken)
    {
        var recorded = await receipts.RecordAsync(request, cancellationToken);
        if (recorded.Detail is { } detail)
            return CreatedAtAction(nameof(Get), new { id = recorded.Id }, detail);

        return Ok(await queries.GetAsync(recorded.Id, cancellationToken));
    }

    [HttpGet, Authorize(Roles = Owners)]
    public async Task<IActionResult> List([FromQuery] ReceiptQuery query, CancellationToken cancellationToken) => Ok(await queries.ListAsync(query, cancellationToken));

    [HttpGet("{id:guid}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) => Ok(await queries.GetAsync(id, cancellationToken));
}
