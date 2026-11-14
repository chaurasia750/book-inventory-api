namespace BookInventory.Application.Core;

/// <summary>Stock D5: writes an empty <c>.xlsx</c> template: one sheet whose first row holds the given column headers.</summary>
public interface ISpreadsheetWriter
{
    byte[] Template(string sheetName, IReadOnlyList<string> headers);
}
