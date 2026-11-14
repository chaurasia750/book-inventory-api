using BookInventory.Domain.Stock;

namespace BookInventory.Application.Stock;

/// <summary>The human text of every <see cref="StockMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class StockMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [StockMessages.ItemNotFound] = "The item could not be found.",
            [StockMessages.NotStockManaged] = "This item is not stock managed, so it has no stock.",
            [StockMessages.InvalidQuantity] = "A quantity must be a whole number greater than zero.",
            [StockMessages.InvalidMovement] = "This movement is not allowed for this stock type.",
            [StockMessages.InvalidDate] = "The movement date is required.",
            [StockMessages.InvalidReason] = "The reason must be 5 to 500 characters.",
            [StockMessages.OpeningExists] = "This item already has an opening stock for this stock type; correct it with an adjustment.",
            [StockMessages.NothingToAdjust] = "The counted quantity equals the current quantity; nothing to adjust.",
            [StockMessages.DocumentAlreadyPosted] = "The stock of this document was already posted.",
            [StockMessages.DocumentNotPosted] = "This document has no stock posted.",
            [StockMessages.DocumentAlreadyReversed] = "The stock of this document was already reversed.",
            [StockMessages.InvalidDocument] = "The document type, id and number are required.",
            [StockMessages.ImportInvalidFile] = "Upload an Excel file (.xlsx) made from the template.",
            [StockMessages.ImportTooLarge] = "The file is larger than 5 MB.",
            [StockMessages.ImportTooManyRows] = "The file has more than 5000 rows.",
            [StockMessages.ImportEmpty] = "The file has no rows to import.",
            [StockMessages.ImportRowsInvalid] = "Some rows are wrong; nothing was imported. Correct them and upload the file again.",
            [StockMessages.ImportItemRequired] = "The item code or barcode is empty.",
            [StockMessages.ImportItemUnknown] = "No item has this code or barcode.",
            [StockMessages.ImportInvalidStockType] = "The stock type must be MAIN, DAMAGED or SAMPLE (empty means MAIN).",
            [StockMessages.ImportDuplicateRow] = "This item and stock type is already in an earlier row of the file."
        };
}
