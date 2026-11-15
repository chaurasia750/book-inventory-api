using BookInventory.Application.School;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.School;

/// <summary>
/// Spec school R19: the company's session cut-offs: read by the Owner, Staff and an entered Super Admin, changed by the Owner and
/// an entered Super Admin.
/// </summary>
[Route("school-settings/session-cut-offs")]
public sealed class SessionCutOffsController(SessionCutOffService cutOffs) : ApiControllerBase
{
    /// <summary>R19: the cut-offs (the defaults until the first change).</summary>
    [HttpGet, Authorize(Roles = "OWNER,STAFF,SUPER_ADMIN")]
    public async Task<IActionResult> Get(CancellationToken cancellationToken) => Ok(await cutOffs.GetAsync(cancellationToken));

    /// <summary>R19: changes both cut-offs.</summary>
    [HttpPut, Authorize(Roles = "OWNER,SUPER_ADMIN")]
    public async Task<IActionResult> Change([FromBody] ChangeSessionCutOffsRequest request, CancellationToken cancellationToken) =>
        Ok(await cutOffs.ChangeAsync(request, cancellationToken));
}
