namespace BookInventory.Application.Core;

/// <summary>One data row of a sheet: its row number in the sheet (the header is row 1) and its cells as text.</summary>
public sealed record SpreadsheetRow(int RowNumber, IReadOnlyList<string> Cells);

/// <summary>
/// The data rows of a sheet. <see cref="IsReadable"/> is false when the file is not a spreadsheet; <see cref="HasTooManyRows"/>
/// is true when it holds more data rows than were asked for (the rows are then not all returned).
/// </summary>
public sealed record SpreadsheetContent(bool IsReadable, bool HasTooManyRows, IReadOnlyList<SpreadsheetRow> Rows);

/// <summary>
/// Stock D5: reads the first sheet of an <c>.xlsx</c> file as text cells, so a use case never sees the Excel package and later
/// imports (school, vendor, item, price) reuse it. Row 1 is the header and is skipped, blank rows are skipped, numbers are
/// written in the invariant culture.
/// </summary>
public interface ISpreadsheetReader
{
    Task<SpreadsheetContent> ReadFirstSheetAsync(Stream content, int maxDataRows, CancellationToken cancellationToken = default);
}
