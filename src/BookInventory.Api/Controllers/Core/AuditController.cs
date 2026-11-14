using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Core;

namespace BookInventory.Api.Controllers.Core;

/// <summary>
/// Core A6-A8: the audit trail, read only. The controller only binds and answers; there is no action that writes. A Super
/// Admin reads every entry, an Owner only those of their own company (the use case applies that from the session); Staff
/// and everyone else are refused here.
/// </summary>
[Route("audit")]
[Authorize(Roles = "SUPER_ADMIN,OWNER")]
public sealed class AuditController(AuditQueryService queries) : ApiControllerBase
{
    /// <summary>A8: the paged entries, newest first, filtered by entity type, entity id, actor, company and a date range.</summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AuditSearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>A8: the entity types present in the entries the caller may read, for the filter.</summary>
    [HttpGet("entity-types")]
    public async Task<IActionResult> EntityTypes(CancellationToken cancellationToken) =>
        Ok(await queries.EntityTypesAsync(cancellationToken));
}
