using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Price;

/// <summary>
/// Spec price R8, R9: the price list and the copy of a version. Reads are open to the Owner, Staff and an entered Super Admin;
/// the copy is for the Owner and the entered Super Admin. The company is never a request field (platform P19, P20).
/// </summary>
[Route("prices")]
public sealed class PricesController(PriceVersionService versions, PriceQueryService queries) : ApiControllerBase
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
}
