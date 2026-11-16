using BookInventory.Application.Report;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Report;

/// <summary>
/// Spec report R1-R13: the dashboard. The Owner and an entered Super Admin see every part; Staff only the orders, the sales and the
/// stock counts (R2). The company is never a request field: the platform takes it from the session.
/// </summary>
[Route("dashboard")]
public sealed class DashboardController(DashboardService dashboard) : ApiControllerBase
{
    /// <summary>R1-R13: the dashboard of a session (<c>?session=2026-27</c>, default the current one).</summary>
    [HttpGet, Authorize(Roles = "OWNER,STAFF,SUPER_ADMIN")]
    public async Task<IActionResult> Get([FromQuery] string? session, CancellationToken cancellationToken) =>
        Ok(await dashboard.GetAsync(new DashboardQuery(session, User.IsInRole("OWNER") || User.IsInRole("SUPER_ADMIN")), cancellationToken));
}
