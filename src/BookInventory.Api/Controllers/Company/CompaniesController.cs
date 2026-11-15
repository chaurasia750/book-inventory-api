using System.ComponentModel.DataAnnotations;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.Api.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BookInventory.Application.Company;
using CompanyRoot = BookInventory.Domain.Company.Company;

namespace BookInventory.Api.Controllers.Company;

/// <summary>
/// R2-R10, R17-R20: the company endpoints. The controller only binds, calls one use case and answers. Everything but
/// <c>companies/current</c> is for the Super Admin only; <c>current</c> answers any authenticated caller with the
/// company of its own session, never of a request field (R6, R10). The invoice identity (R17-R20) is set by the
/// company's Owner or a Super Admin inside the company, and read by anyone with a company context.
/// </summary>
[Route("companies")]
public sealed class CompaniesController(
    CompanyRegistrationService registration,
    CompanyLifecycleService lifecycle,
    CompanyQueryService queries,
    CompanyEntryService entry,
    CompanyInvoiceIdentityService identity,
    SessionCookieWriter cookies) : ApiControllerBase
{
    private const string IdentityWriters = "OWNER,SUPER_ADMIN";
    private const long ImageBodyLimit = CompanyRoot.ImageMaxBytes + (1024 * 1024);

    /// <summary>R3/R4: creates a company with its Owner; the Owner's user id and temporary password are shown once.</summary>
    [HttpPost, Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateCompanyRequest request, CancellationToken cancellationToken)
    {
        var created = await registration.CreateAsync(request, cancellationToken);
        Response.Headers.CacheControl = "no-store";   // the answer carries the temporary password, shown once (R4)
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    /// <summary>R9: the paged company list with search, status filter and a whitelisted sort.</summary>
    [HttpGet, Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> List([FromQuery] CompanySearchQuery query, CancellationToken cancellationToken) =>
        Ok(await queries.SearchAsync(query, cancellationToken));

    /// <summary>R9/R11: one company with its status timeline and its accounts.</summary>
    [HttpGet("{id:guid}"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await queries.GetAsync(id, cancellationToken));

    /// <summary>R3: edits the master data.</summary>
    [HttpPut("{id:guid}"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Edit(Guid id, [FromBody] EditCompanyRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.EditAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the company off with a reason and ends every session of its accounts.</summary>
    [HttpPost("{id:guid}/deactivate"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Deactivate(Guid id, [FromBody] DeactivateCompanyRequest request, CancellationToken cancellationToken)
    {
        await lifecycle.DeactivateAsync(id, request, cancellationToken);
        return NoContent();
    }

    /// <summary>R7: switches the company on again; the note is optional.</summary>
    [HttpPost("{id:guid}/activate"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Activate(Guid id, [FromBody] ActivateCompanyRequest? request, CancellationToken cancellationToken)
    {
        await lifecycle.ReactivateAsync(id, request ?? new ActivateCompanyRequest(), cancellationToken);
        return NoContent();
    }

    /// <summary>R13: a Super Admin enters an active company; the answer carries the company and the access token that carries it.</summary>
    [HttpPost("{id:guid}/enter"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Enter(Guid id, CancellationToken cancellationToken)
    {
        var entered = await entry.EnterAsync(id, cancellationToken);
        cookies.WriteAccess(Response, entered);
        return Ok(entered);
    }

    /// <summary>R13: a Super Admin leaves the company they entered and has no company again.</summary>
    [HttpPost("leave"), Authorize(Roles = "SUPER_ADMIN")]
    public async Task<IActionResult> Leave(CancellationToken cancellationToken)
    {
        var left = await entry.LeaveAsync(cancellationToken);
        cookies.WriteAccess(Response, left);
        return Ok(left);
    }

    /// <summary>R6/R10: the company of this request's session (a company account's own, or the one a Super Admin entered).</summary>
    [HttpGet("current")]
    public async Task<IActionResult> Current(CancellationToken cancellationToken) =>
        Ok(await queries.GetCurrentAsync(cancellationToken));

    /// <summary>R20: the invoice identity of the session's company; empty values when nothing is set, <c>403</c> without a company context.</summary>
    [HttpGet("current/invoice-identity")]
    public async Task<IActionResult> CurrentInvoiceIdentity(CancellationToken cancellationToken) =>
        Ok(await queries.GetInvoiceIdentityAsync(cancellationToken));

    /// <summary>R19/R20: the Owner (or a Super Admin inside the company) sets the one bank account and the invoice settings.</summary>
    [HttpPut("{id:guid}/invoice-identity"), Authorize(Roles = IdentityWriters)]
    public async Task<IActionResult> SetInvoiceIdentity(Guid id, [FromBody] SetCompanyInvoiceIdentityRequest request, CancellationToken cancellationToken)
    {
        await identity.SetAsync(id, request, cancellationToken);
        return Ok();
    }

    /// <summary>R17: uploads or replaces the logo; a PNG or JPEG up to 2 MB, else <c>400 company.logo_invalid</c>.</summary>
    [HttpPost("{id:guid}/logo"), Authorize(Roles = IdentityWriters)]
    [RequestSizeLimit(ImageBodyLimit), RequestFormLimits(MultipartBodyLengthLimit = ImageBodyLimit)]
    public async Task<IActionResult> UploadLogo(Guid id, [Required] IFormFile file, CancellationToken cancellationToken)
    {
        await identity.SetLogoAsync(id, await ReadImageAsync(file, cancellationToken), cancellationToken);
        return Ok();
    }

    /// <summary>R17: removes the logo.</summary>
    [HttpDelete("{id:guid}/logo"), Authorize(Roles = IdentityWriters)]
    public async Task<IActionResult> RemoveLogo(Guid id, CancellationToken cancellationToken)
    {
        await identity.RemoveLogoAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R17: the logo bytes with the stored content type; <c>404</c> when there is none.</summary>
    [HttpGet("{id:guid}/logo")]
    public Task<IActionResult> Logo(Guid id, CancellationToken cancellationToken) =>
        ImageAsync(id, CompanyImageKind.Logo, cancellationToken);

    /// <summary>R18: uploads or replaces the signature; a PNG or JPEG up to 2 MB, else <c>400 company.signature_invalid</c>.</summary>
    [HttpPost("{id:guid}/signature"), Authorize(Roles = IdentityWriters)]
    [RequestSizeLimit(ImageBodyLimit), RequestFormLimits(MultipartBodyLengthLimit = ImageBodyLimit)]
    public async Task<IActionResult> UploadSignature(Guid id, [Required] IFormFile file, CancellationToken cancellationToken)
    {
        await identity.SetSignatureAsync(id, await ReadImageAsync(file, cancellationToken), cancellationToken);
        return Ok();
    }

    /// <summary>R18: removes the signature.</summary>
    [HttpDelete("{id:guid}/signature"), Authorize(Roles = IdentityWriters)]
    public async Task<IActionResult> RemoveSignature(Guid id, CancellationToken cancellationToken)
    {
        await identity.RemoveSignatureAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>R18: the signature bytes with the stored content type; <c>404</c> when there is none.</summary>
    [HttpGet("{id:guid}/signature")]
    public Task<IActionResult> Signature(Guid id, CancellationToken cancellationToken) =>
        ImageAsync(id, CompanyImageKind.Signature, cancellationToken);

    private async Task<IActionResult> ImageAsync(Guid id, CompanyImageKind kind, CancellationToken cancellationToken)
    {
        var image = await queries.GetImageAsync(id, kind, cancellationToken);
        Response.Headers.CacheControl = "private, max-age=300";   // the image changes rarely; the browser may cache it briefly
        return File(image.Content, image.ContentType);
    }

    private static async Task<byte[]> ReadImageAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var source = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
