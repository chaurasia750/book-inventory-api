using System.Globalization;
using BookInventory.Application.Core;
using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>
/// R17: one row of the opening stock file, read from its three text cells: the item code or barcode, the stock type (empty means
/// Main) and the quantity (a whole number in the range of <see cref="ItemStock.IsValidQuantity"/>). A cell that cannot be read
/// gives the row its first error; the item itself is checked later, against the item directory.
/// </summary>
public sealed record OpeningImportRow(int RowNumber, string Key, StockType StockType, int Quantity)
{
    public const string KeyColumn = "ItemCodeOrBarcode";
    public const string StockTypeColumn = "StockType";
    public const string QuantityColumn = "Quantity";

    public static IReadOnlyList<string> Headers { get; } = ["Item code or barcode", "Stock type", "Quantity"];

    /// <summary>The row, or its first format error.</summary>
    public static (OpeningImportRow? Row, OpeningImportRowError? Error) Parse(SpreadsheetRow row)
    {
        var key = Cell(row, 0).ToUpperInvariant();
        if (key.Length == 0)
            return (null, new OpeningImportRowError(row.RowNumber, KeyColumn, StockMessages.ImportItemRequired));
        if (!TryStockType(Cell(row, 1), out var stockType))
            return (null, new OpeningImportRowError(row.RowNumber, StockTypeColumn, StockMessages.ImportInvalidStockType));
        if (!TryQuantity(Cell(row, 2), out var quantity))
            return (null, new OpeningImportRowError(row.RowNumber, QuantityColumn, StockMessages.InvalidQuantity));

        return (new OpeningImportRow(row.RowNumber, key, stockType, quantity), null);
    }

    public OpeningImportRowError ErrorOf(string column, string code) => new(RowNumber, column, code);

    private static string Cell(SpreadsheetRow row, int index) => index < row.Cells.Count ? row.Cells[index].Trim() : string.Empty;

    private static bool TryStockType(string text, out StockType stockType)
    {
        stockType = StockType.Main;
        return text.Length == 0
               || (Enum.TryParse(text, ignoreCase: true, out stockType) && Enum.IsDefined(stockType) && !int.TryParse(text, out _));
    }

    private static bool TryQuantity(string text, out int quantity)
    {
        quantity = 0;
        if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
            || value != decimal.Truncate(value) || value is < int.MinValue or > int.MaxValue)
            return false;

        quantity = (int)value;
        return ItemStock.IsValidQuantity(quantity);
    }
}
