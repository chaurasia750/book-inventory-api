using System.ComponentModel.DataAnnotations;
using BookInventory.Application.Core;
using BookInventory.Application.Item;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.Domain.Item;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Item;

/// <summary>
/// Spec item R18-R22: the Excel template of items and its upload, for the Owner and a Super Admin who entered the company. A
/// file with any wrong row answers <c>400</c> with the problem code <c>item.import.rows_invalid</c> and the report in
/// <c>rows</c>; the answer rolls the request transaction back, so nothing is changed then. The request body is limited a little
/// above the 5 MB file limit, so a slightly larger file still gets the module's own message.
/// </summary>
[Route("items/import")]
public sealed class ItemImportController(ItemImportService import, ISpreadsheetWriter writer, IMessageCatalog catalog) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const long BodyLimit = ItemImportService.MaxFileBytes + (1024 * 1024);
    private const string XlsxType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>R18: the template with the ten columns.</summary>
    [HttpGet("template"), Authorize(Roles = Writers)]
    public IActionResult Template() =>
        File(writer.Template("Items", ItemImportRow.Headers), XlsxType, "items-template.xlsx");

    /// <summary>R18-R22: creates and updates every row or none.</summary>
    [HttpPost, Authorize(Roles = Writers)]
    [RequestSizeLimit(BodyLimit), RequestFormLimits(MultipartBodyLengthLimit = BodyLimit)]
    public async Task<IActionResult> Import([Required] IFormFile file, CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        var result = await import.ImportAsync(new ItemImportFile(file.FileName, file.Length, content), cancellationToken);
        if (result.IsValid)
            return Ok(new { created = result.Created, updated = result.Updated });

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = catalog.Find(ItemMessages.ImportRowsInvalid)
        };
        problem.Extensions["code"] = ItemMessages.ImportRowsInvalid;
        problem.Extensions["rows"] = result.Errors;
        return BadRequest(problem);
    }
}
