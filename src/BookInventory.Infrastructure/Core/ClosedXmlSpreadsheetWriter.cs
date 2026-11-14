using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using ClosedXML.Excel;

namespace BookInventory.Infrastructure.Core;

/// <summary>Stock D5: the ClosedXML adapter of <see cref="ISpreadsheetWriter"/>: a template with a bold header row.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISpreadsheetWriter))]
public sealed class ClosedXmlSpreadsheetWriter : ISpreadsheetWriter
{
    public byte[] Template(string sheetName, IReadOnlyList<string> headers)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.AddWorksheet(sheetName);
        for (var i = 0; i < headers.Count; i++)
            sheet.Cell(1, i + 1).Value = headers[i];
        sheet.Row(1).Style.Font.Bold = true;
        sheet.Columns(1, headers.Count).Width = 24;

        using var output = new MemoryStream();
        workbook.SaveAs(output);
        return output.ToArray();
    }
}
