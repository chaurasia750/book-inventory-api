using BookInventory.Application.Finance;
using BookInventory.BuildingBlocks.Configuration.Controller;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Finance;

/// <summary>
/// Spec finance R9, R13: the statement of one school or one vendor, for the Owner and a Super Admin who entered the company. Staff
/// have no access; nothing posts through these routes (postings come only from the documents, R5).
/// </summary>
[Authorize(Roles = Readers)]
public sealed class PartyLedgerController(LedgerStatementService statements, LedgerPdfService pdfs) : ApiControllerBase
{
    private const string Readers = "OWNER,SUPER_ADMIN";

    /// <summary>R9: a school's statement.</summary>
    [HttpGet("schools/{schoolId:guid}/ledger")]
    public async Task<IActionResult> School(Guid schoolId, [FromQuery] LedgerStatementQuery query, CancellationToken cancellationToken) =>
        Ok(await statements.SchoolStatementAsync(schoolId, query, cancellationToken));

    /// <summary>R9: a vendor's statement (no session filter).</summary>
    [HttpGet("vendors/{vendorId:guid}/ledger")]
    public async Task<IActionResult> Vendor(Guid vendorId, [FromQuery] LedgerStatementQuery query, CancellationToken cancellationToken) =>
        Ok(await statements.VendorStatementAsync(vendorId, query, cancellationToken));

    /// <summary>R18: a school statement as an A4 PDF, rendered on demand and never stored.</summary>
    [HttpGet("schools/{schoolId:guid}/ledger/pdf")]
    public async Task<IActionResult> SchoolPdf(Guid schoolId, [FromQuery] LedgerStatementQuery query, CancellationToken cancellationToken) =>
        Download(await pdfs.SchoolAsync(schoolId, query, cancellationToken));

    /// <summary>R18: a vendor statement as an A4 PDF.</summary>
    [HttpGet("vendors/{vendorId:guid}/ledger/pdf")]
    public async Task<IActionResult> VendorPdf(Guid vendorId, [FromQuery] LedgerStatementQuery query, CancellationToken cancellationToken) =>
        Download(await pdfs.VendorAsync(vendorId, query, cancellationToken));

    private FileContentResult Download(LedgerPdf pdf)
    {
        Response.Headers.ContentDisposition = $"inline; filename=\"{pdf.FileName}\"";
        return File(pdf.Content, "application/pdf");
    }
}
