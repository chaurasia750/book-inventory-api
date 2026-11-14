using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Iam.History;

namespace BookInventory.Api.Controllers.Iam;

/// <summary>
/// R26: the Super Admin's view of the whole login log - a filtered, sorted, paged search and a CSV export of the same
/// filters. Super Admin only; every other role gets 403. Rows of the hidden system account come back under the fixed
/// label of R48 (no user name, no id), and the export escapes every value against spreadsheet formulas.
/// </summary>
[Route("login-logs")]
[Authorize(Roles = "SUPER_ADMIN")]
public sealed class LoginLogsController(LoginHistoryService history) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] LoginLogQuery query, CancellationToken cancellationToken) =>
        Ok(await history.SearchAsync(query, cancellationToken));

    /// <summary>The same filters as a CSV file, or 400 <c>iam.login_log.export_too_large</c> when too many rows match.</summary>
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] LoginLogQuery query, CancellationToken cancellationToken)
    {
        var csv = await history.ExportCsvAsync(query, cancellationToken);
        Response.Headers.CacheControl = "no-store";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv; charset=utf-8", $"login-log-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv");
    }
}
