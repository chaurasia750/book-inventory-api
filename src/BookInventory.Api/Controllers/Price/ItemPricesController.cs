using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Price;

/// <summary>
/// Spec price R1-R6: the price versions of one item. The controller only binds, calls one use case and answers. Reads are open to
/// the Owner, Staff (without the purchase price, applied in the query service) and a Super Admin who entered the company; writes
/// are for the Owner and the entered Super Admin. The company is never a request field (platform P19, P20).
/// </summary>
[Route("items/{itemId:guid}/prices")]
public sealed class ItemPricesController(PriceVersionService versions, PriceQueryService queries) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R4, R5: the item's versions, newest first, with the default marked.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List(Guid itemId, CancellationToken cancellationToken) =>
        Ok(await queries.VersionsOfItemAsync(itemId, cancellationToken));

    /// <summary>R2, R3: adds a version.</summary>
    [HttpPost, Authorize(Roles = Writers)]
    public async Task<IActionResult> Add(Guid itemId, [FromBody] AddPriceVersionRequest request, CancellationToken cancellationToken)
    {
        var created = await versions.AddAsync(itemId, request, cancellationToken);
        return CreatedAtAction(nameof(List), new { itemId }, created);
    }

    /// <summary>R3: edits a version in place.</summary>
    [HttpPut("{versionId:guid}"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Edit(Guid itemId, Guid versionId, [FromBody] EditPriceVersionRequest request, CancellationToken cancellationToken)
    {
        await versions.EditAsync(itemId, versionId, request, cancellationToken);
        return NoContent();
    }
}
