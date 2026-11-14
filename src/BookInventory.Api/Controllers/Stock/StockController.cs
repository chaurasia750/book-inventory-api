using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Stock;

/// <summary>
/// Spec stock R9, R10: the current stock list of the company. Read by the Owner, Staff and a Super Admin who entered the company;
/// the company is never a request field (platform P19, P20).
/// </summary>
[Route("stock")]
public sealed class StockController(StockQueryService queries) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";

    /// <summary>R10: paged, searchable, filterable, sortable; each row has the three quantities and the warning mark.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> List([FromQuery] StockListQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.ListAsync(query, cancellationToken));
}
