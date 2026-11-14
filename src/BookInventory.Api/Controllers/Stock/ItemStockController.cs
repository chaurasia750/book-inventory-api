using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Stock;

/// <summary>
/// Spec stock R7, R8, R10: one item's stock: its quantities and history (Owner, Staff, entered Super Admin), the opening and the
/// adjustment (Owner and entered Super Admin). Entries are never changed or deleted, so there is no such route (R4).
/// </summary>
[Route("items/{itemId:guid}/stock")]
public sealed class ItemStockController(StockLedgerService ledger, StockQueryService queries) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R10: the three quantities and the entries newest first with the quantity after each.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid itemId, CancellationToken cancellationToken) =>
        Ok(await queries.ItemStockAsync(itemId, cancellationToken));

    /// <summary>R7: the opening of one stock type.</summary>
    [HttpPost("opening"), Authorize(Roles = Writers)]
    public async Task<IActionResult> EnterOpening(Guid itemId, [FromBody] EnterOpeningRequest request, CancellationToken cancellationToken)
    {
        var posted = await ledger.EnterOpeningAsync(itemId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { itemId }, posted);
    }

    /// <summary>R8: the counted quantity of one stock type; the difference is posted.</summary>
    [HttpPost("adjustment"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Adjust(Guid itemId, [FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var posted = await ledger.AdjustAsync(itemId, request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { itemId }, posted);
    }
}
