using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>
/// Spec sales R26: the layout a school's invoices open in. The Owner, Staff and an entered Super Admin read it; the Owner and an
/// entered Super Admin change it.
/// </summary>
[Route("schools/{schoolId:guid}/invoice-layout")]
public sealed class InvoiceLayoutsController(InvoiceLayoutService layouts) : ApiControllerBase
{
    [HttpGet, Authorize(Roles = "OWNER,STAFF,SUPER_ADMIN")]
    public async Task<IActionResult> Get(Guid schoolId, CancellationToken cancellationToken) => Ok(await layouts.GetAsync(schoolId, cancellationToken));

    [HttpPut, Authorize(Roles = "OWNER,SUPER_ADMIN")]
    public async Task<IActionResult> Change(Guid schoolId, [FromBody] ChangeInvoiceLayoutRequest request, CancellationToken cancellationToken) =>
        Ok(await layouts.ChangeAsync(schoolId, request, cancellationToken));
}
