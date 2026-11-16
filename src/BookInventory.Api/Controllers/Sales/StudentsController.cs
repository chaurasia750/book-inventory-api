using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Sales;

/// <summary>Spec sales R41, R48: the student suggestion of the school counter, for the Owner, Staff and an entered Super Admin.</summary>
[Route("students")]
[Authorize(Roles = "OWNER,STAFF,SUPER_ADMIN")]
public sealed class StudentsController(StudentQueryService queries) : ApiControllerBase
{
    /// <summary>R41: at most 20 students of the school whose name contains the text; under two characters answers an empty list.</summary>
    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] StudentSuggestQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SuggestAsync(query, cancellationToken));
}
