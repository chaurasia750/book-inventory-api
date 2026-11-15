using BookInventory.Application.Planning;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Planning;

/// <summary>
/// Spec planning R2-R7: a school's class-wise book lists. The Owner, Staff and an entered Super Admin read them; the Owner and an
/// entered Super Admin create, change, copy and delete them. The company is never a request field.
/// </summary>
[Route("schools/{schoolId:guid}/book-lists")]
public sealed class BookListsController(ClassBookListService lists, ClassBookListQueryService queries) : ApiControllerBase
{
    private const string Readers = "OWNER,STAFF,SUPER_ADMIN";
    private const string Owners = "OWNER,SUPER_ADMIN";

    /// <summary>R7: every class of the school for a session (the school's current one when empty), compared with the session before.</summary>
    [HttpGet, Authorize(Roles = Readers)]
    public async Task<IActionResult> Get(Guid schoolId, [FromQuery] string? session, CancellationToken cancellationToken) =>
        Ok(await queries.SessionListsAsync(schoolId, session, cancellationToken));

    /// <summary>R2, R3, R5: creates or replaces one class's list.</summary>
    [HttpPut("{session}/{className}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Save([FromRoute] BookListRoute route, [FromBody] SaveBookListRequest request, CancellationToken cancellationToken)
    {
        await lists.SaveAsync(route.ToKey(), request, cancellationToken);
        return NoContent();
    }

    /// <summary>R5: removes one class's list.</summary>
    [HttpDelete("{session}/{className}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Delete([FromRoute] BookListRoute route, CancellationToken cancellationToken)
    {
        await lists.DeleteAsync(route.ToKey(), cancellationToken);
        return NoContent();
    }

    /// <summary>R4: copies a session's lists (this school's or another school's) into this session.</summary>
    [HttpPost("{session}/copy"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Copy(Guid schoolId, string session, [FromBody] CopyBookListsRequest request, CancellationToken cancellationToken) =>
        Ok(await lists.CopyAsync(schoolId, session, request, cancellationToken));
}
