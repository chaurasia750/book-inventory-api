using System.Globalization;
using BookInventory.Application.Core;
using BookInventory.Domain.Item;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R18-R21: one row of the item file, read from its ten text cells. A cell that cannot be read gives the row its first error; the
/// values the item aggregate checks itself (name length, HSN, GST rate, barcode shape) are checked by the aggregate when the row
/// is applied. The class is written as the standard class name when it matches one ignoring case (R20).
/// </summary>
public sealed record ItemImportRow(
    int RowNumber,
    string? ItemCode,
    string? Barcode,
    string? Name,
    ItemCategory? Category,
    ImportCell ClassName,
    ImportCell Subject,
    ImportCell HsnCode,
    decimal? GstRatePercent,
    ImportCell VendorName,
    bool? IsStockManaged)
{
    public const string ItemCodeColumn = "ItemCode";
    public const string BarcodeColumn = "Barcode";
    public const string NameColumn = "Name";
    public const string CategoryColumn = "Category";
    public const string ClassColumn = "Class";
    public const string SubjectColumn = "Subject";
    public const string HsnColumn = "HsnCode";
    public const string GstColumn = "GstRate";
    public const string VendorColumn = "Vendor";
    public const string StockManagedColumn = "StockManaged";

    public static IReadOnlyList<string> Headers { get; } =
        ["Item code", "Barcode", "Name", "Category", "Class", "Subject", "HSN code", "GST rate", "Vendor", "Stock managed"];

    /// <summary>The row, or its first format error.</summary>
    public static (ItemImportRow? Row, ItemImportRowError? Error) Parse(SpreadsheetRow row)
    {
        var category = Cell(row, 3);
        var gst = Cell(row, 7);
        var stock = Cell(row, 9);
        var classCell = ImportCell.Of(Cell(row, 4));
        var subject = ImportCell.Of(Cell(row, 5));

        if (category == ImportCell.ClearMark || Cell(row, 2) == ImportCell.ClearMark)
            return Fail(row, category == ImportCell.ClearMark ? CategoryColumn : NameColumn, ItemMessages.ImportCannotClear);
        if (!TryCategory(category, out var parsedCategory))
            return Fail(row, CategoryColumn, ItemMessages.ImportInvalidCategory);
        if (!TryGst(gst, out var parsedGst))
            return Fail(row, GstColumn, ItemMessages.ImportInvalidNumber);
        if (!TryYesNo(stock, out var parsedStock))
            return Fail(row, StockManagedColumn, ItemMessages.ImportInvalidStockManaged);
        if (TooLong(classCell, ItemRoot.ClassMaxLength))
            return Fail(row, ClassColumn, ItemMessages.InvalidField);
        if (TooLong(subject, ItemRoot.SubjectMaxLength))
            return Fail(row, SubjectColumn, ItemMessages.InvalidField);

        var parsed = new ItemImportRow(
            row.RowNumber, NullIfEmpty(Cell(row, 0))?.ToUpperInvariant(), NullIfEmpty(Cell(row, 1))?.ToUpperInvariant(), NullIfEmpty(Cell(row, 2)),
            parsedCategory, Standardised(classCell), subject, ImportCell.Of(Cell(row, 6)), parsedGst, ImportCell.Of(Cell(row, 8)), parsedStock);
        return (parsed, null);
    }

    public ItemImportRowError ErrorOf(string column, string code) => new(RowNumber, column, code);

    private static (ItemImportRow?, ItemImportRowError?) Fail(SpreadsheetRow row, string column, string code) =>
        (null, new ItemImportRowError(row.RowNumber, column, code));

    private static string Cell(SpreadsheetRow row, int index) => index < row.Cells.Count ? row.Cells[index].Trim() : string.Empty;

    private static string? NullIfEmpty(string text) => text.Length == 0 ? null : text;

    private static bool TooLong(ImportCell cell, int maxLength) => cell.Value is { } value && value.Length > maxLength;

    private static ImportCell Standardised(ImportCell cell) =>
        cell.Value is { } value
            ? new ImportCell(false, Domain.School.School.DefaultClasses.FirstOrDefault(c => string.Equals(c, value, StringComparison.OrdinalIgnoreCase)) ?? value)
            : cell;

    private static bool TryCategory(string text, out ItemCategory? category)
    {
        category = null;
        if (text.Length == 0)
            return true;
        if (int.TryParse(text, out _) || !Enum.TryParse<ItemCategory>(text, ignoreCase: true, out var parsed) || !Enum.IsDefined(parsed))
            return false;

        category = parsed;
        return true;
    }

    private static bool TryGst(string text, out decimal? percent)
    {
        percent = null;
        if (text.Length == 0)
            return true;
        if (!decimal.TryParse(text.TrimEnd('%'), NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return false;

        percent = value;
        return true;
    }

    private static bool TryYesNo(string text, out bool? value)
    {
        value = null;
        if (text.Length == 0)
            return true;
        if (text.Equals("yes", StringComparison.OrdinalIgnoreCase) || text.Equals("y", StringComparison.OrdinalIgnoreCase))
            value = true;
        else if (text.Equals("no", StringComparison.OrdinalIgnoreCase) || text.Equals("n", StringComparison.OrdinalIgnoreCase))
            value = false;

        return value is not null;
    }
}
