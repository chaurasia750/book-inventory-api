namespace BookInventory.Domain.Stock;

/// <summary>
/// Every message code of the <c>stock</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the
/// human text lives in <c>BookInventory.Application.Stock.StockMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class StockMessages
{
    public const string ItemNotFound = "stock.item_not_found";
    public const string NotStockManaged = "stock.not_stock_managed";
    public const string InvalidQuantity = "stock.invalid_quantity";
    public const string InvalidMovement = "stock.invalid_movement";
    public const string InvalidDate = "stock.invalid_date";
    public const string InvalidReason = "stock.invalid_reason";
    public const string OpeningExists = "stock.opening_exists";
    public const string NothingToAdjust = "stock.nothing_to_adjust";
    public const string DocumentAlreadyPosted = "stock.document_already_posted";
    public const string DocumentNotPosted = "stock.document_not_posted";
    public const string DocumentAlreadyReversed = "stock.document_already_reversed";
    public const string InvalidDocument = "stock.invalid_document";
    public const string ImportInvalidFile = "stock.import.invalid_file";
    public const string ImportTooLarge = "stock.import.too_large";
    public const string ImportTooManyRows = "stock.import.too_many_rows";
    public const string ImportEmpty = "stock.import.empty";
    public const string ImportRowsInvalid = "stock.import.rows_invalid";
    public const string ImportItemRequired = "stock.import.item_required";
    public const string ImportItemUnknown = "stock.import.item_unknown";
    public const string ImportInvalidStockType = "stock.import.invalid_stock_type";
    public const string ImportDuplicateRow = "stock.import.duplicate_row";
}
