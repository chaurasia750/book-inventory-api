using System.ComponentModel.DataAnnotations;
using BookInventory.Application.Core;
using BookInventory.Application.Stock;
using BookInventory.BuildingBlocks.Configuration.Controller;
using BookInventory.BuildingBlocks.Domain;
using BookInventory.Domain.Stock;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BookInventory.Api.Controllers.Stock;

/// <summary>
/// Spec stock R17-R19: the Excel opening stock template and its upload, for the Owner and a Super Admin who entered the company.
/// A file with any wrong row answers <c>400</c> with the problem code <c>stock.import.rows_invalid</c> and the report in
/// <c>rows</c>; nothing is imported then. The request body is limited a little above the 5 MB file limit, so a slightly larger
/// file still gets the module's own message.
/// </summary>
[Route("stock/opening")]
public sealed class OpeningStockImportController(OpeningStockImportService import, ISpreadsheetWriter writer, IMessageCatalog catalog) : ApiControllerBase
{
    private const string Writers = "OWNER,SUPER_ADMIN";
    private const long BodyLimit = OpeningStockImportService.MaxFileBytes + (1024 * 1024);
    private const string XlsxType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>R17: the template with the three columns.</summary>
    [HttpGet("template"), Authorize(Roles = Writers)]
    public IActionResult Template() =>
        File(writer.Template("Opening stock", OpeningImportRow.Headers), XlsxType, "opening-stock-template.xlsx");

    /// <summary>R17, R18: imports every row or none.</summary>
    [HttpPost("import"), Authorize(Roles = Writers)]
    [RequestSizeLimit(BodyLimit), RequestFormLimits(MultipartBodyLengthLimit = BodyLimit)]
    public async Task<IActionResult> Import([Required] IFormFile file, [FromForm, Required] DateOnly? openingDate, CancellationToken cancellationToken)
    {
        await using var content = file.OpenReadStream();
        var result = await import.ImportAsync(new OpeningImportFile(file.FileName, file.Length, content), openingDate!.Value, cancellationToken);
        if (result.IsValid)
            return Ok(result);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = catalog.Find(StockMessages.ImportRowsInvalid)
        };
        problem.Extensions["code"] = StockMessages.ImportRowsInvalid;
        problem.Extensions["rows"] = result.Errors;
        return BadRequest(problem);
    }
}
