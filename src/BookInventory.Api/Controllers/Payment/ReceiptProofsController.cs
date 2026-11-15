using System.ComponentModel.DataAnnotations;
using BookInventory.Application.Payment;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.Domain.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Payment;

/// <summary>
/// Spec payment R20: the optional proof files of a receipt, for the Owner and a Super Admin who entered the company. The request
/// body is limited a little above the 5 MB file limit, so a slightly larger file still gets the module's own message.
/// </summary>
[Route("receipts/{receiptId:guid}/proofs")]
public sealed class ReceiptProofsController(ReceiptProofService proofs) : ApiControllerBase
{
    private const string Owners = "OWNER,SUPER_ADMIN";
    private const long BodyLimit = ReceiptProof.MaxBytes + (1024 * 1024);

    /// <summary>R20: attaches a proof (PNG, JPEG or PDF, at most 5 MB, at most 5 per receipt).</summary>
    [HttpPost, Authorize(Roles = Owners)]
    [RequestSizeLimit(BodyLimit), RequestFormLimits(MultipartBodyLengthLimit = BodyLimit)]
    public async Task<IActionResult> Attach(Guid receiptId, [Required] IFormFile file, CancellationToken cancellationToken)
    {
        await using var source = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await source.CopyToAsync(buffer, cancellationToken);
        var proof = await proofs.AttachAsync(receiptId, file.FileName, buffer.ToArray(), cancellationToken);
        return CreatedAtAction(nameof(Read), new { receiptId, proofId = proof.Id }, proof);
    }

    /// <summary>R20: the proof file with its stored content type.</summary>
    [HttpGet("{proofId:guid}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Read(Guid receiptId, Guid proofId, CancellationToken cancellationToken)
    {
        var file = await proofs.ReadAsync(receiptId, proofId, cancellationToken);
        Response.Headers.CacheControl = "private, max-age=300";
        return File(file.Content, file.ContentType);
    }

    /// <summary>R20: removes a proof.</summary>
    [HttpDelete("{proofId:guid}"), Authorize(Roles = Owners)]
    public async Task<IActionResult> Remove(Guid receiptId, Guid proofId, CancellationToken cancellationToken)
    {
        await proofs.RemoveAsync(receiptId, proofId, cancellationToken);
        return NoContent();
    }
}
