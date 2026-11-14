using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Price;

/// <summary>
/// Spec price R8-R13: the price list, the copy of a version and the bulk price change. Reads are open to the Owner, Staff and an entered Super Admin;
/// the copy and the bulk change are for the Owner and the entered Super Admin. The company is never a request field (platform P19, P20).
/// </summary>
[Route("prices")]
public sealed class PricesController(PriceVersionService versions, PriceQueryService queries, BulkPriceChangeService bulk) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R9: the paged price list, one row per item with its default version.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] PriceListQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.ListAsync(query, cancellationToken));

    /// <summary>R8: copies a version to a new name and date for the selected items.</summary>
    [HttpPost("copy"), Authorize(Roles = Writers)]
    public async Task<IActionResult> Copy([FromBody] CopyPriceVersionRequest request, CancellationToken cancellationToken) =>
        Ok(await versions.CopyAsync(request, cancellationToken));

    /// <summary>R10-R12: shows what a bulk change would do, page by page, and saves nothing.</summary>
    [HttpPost("bulk/preview"), Authorize(Roles = Writers)]
    public async Task<IActionResult> PreviewBulk([FromBody] BulkPriceChangeRequest request, CancellationToken cancellationToken) =>
        Ok(await bulk.PreviewAsync(request, cancellationToken));

    /// <summary>R13: applies a bulk change, recomputed on the server, all or nothing.</summary>
    [HttpPost("bulk/apply"), Authorize(Roles = Writers)]
    public async Task<IActionResult> ApplyBulk([FromBody] BulkPriceChangeRequest request, CancellationToken cancellationToken) =>
        Ok(await bulk.ApplyAsync(request, cancellationToken));
}
