using System.Globalization;
using BookInventory.Application.Core;
using BookInventory.BuildingBlocks.Application.Attributes;
using ClosedXML.Excel;

namespace BookInventory.Infrastructure.Core;

/// <summary>
/// Stock D5: the ClosedXML adapter of <see cref="ISpreadsheetReader"/>. Only this class and <see cref="ClosedXmlSpreadsheetWriter"/>
/// know the package; cells come back as text (numbers in the invariant culture), so a use case parses them itself.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISpreadsheetReader))]
public sealed class ClosedXmlSpreadsheetReader : ISpreadsheetReader
{
    public async Task<SpreadsheetContent> ReadFirstSheetAsync(Stream content, int maxDataRows, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;

        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(buffer);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Not an .xlsx (a renamed CSV, a broken file): the caller refuses the file with its own message code.
            return new SpreadsheetContent(false, false, []);
        }

        using (workbook)
            return Read(workbook.Worksheets.First(), maxDataRows);
    }

    private static SpreadsheetContent Read(IXLWorksheet sheet, int maxDataRows)
    {
        var rows = new List<SpreadsheetRow>();
        var width = Math.Max(sheet.LastColumnUsed()?.ColumnNumber() ?? 0, 1);
        foreach (var row in sheet.RowsUsed().Where(r => r.RowNumber() > 1))
        {
            var cells = Enumerable.Range(1, width).Select(c => TextOf(row.Cell(c))).ToList();
            if (cells.All(string.IsNullOrWhiteSpace))
                continue;
            if (rows.Count == maxDataRows)
                return new SpreadsheetContent(true, true, rows);
            rows.Add(new SpreadsheetRow(row.RowNumber(), cells));
        }

        return new SpreadsheetContent(true, false, rows);
    }

    private static string TextOf(IXLCell cell)
    {
        var value = cell.Value;
        if (value.IsBlank) return string.Empty;
        if (value.IsNumber) return value.GetNumber().ToString(CultureInfo.InvariantCulture);
        if (value.IsText) return value.GetText();
        return cell.GetFormattedString();
    }
}
