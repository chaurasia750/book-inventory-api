namespace BookInventory.Domain.Item;

/// <summary>
/// Every message code of the <c>item</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the
/// human text lives in <c>BookInventory.Application.Item.ItemMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class ItemMessages
{
    public const string NotFound = "item.not_found";
    public const string InvalidName = "item.invalid_name";
    public const string InvalidField = "item.invalid_field";
    public const string InvalidHsn = "item.invalid_hsn";
    public const string InvalidGstRate = "item.invalid_gst_rate";
    public const string InvalidVendor = "item.invalid_vendor";
    public const string InvalidBarcode = "item.invalid_barcode";
    public const string DuplicateBarcode = "item.duplicate_barcode";
    public const string BarcodeTaken = "item.barcode_taken";
    public const string ImportInvalidFile = "item.import.invalid_file";
    public const string ImportTooLarge = "item.import.too_large";
    public const string ImportTooManyRows = "item.import.too_many_rows";
    public const string ImportEmpty = "item.import.empty";
    public const string ImportRowsInvalid = "item.import.rows_invalid";
    public const string ImportItemUnknown = "item.import.item_unknown";
    public const string ImportDuplicateRow = "item.import.duplicate_row";
    public const string ImportInvalidCategory = "item.import.invalid_category";
    public const string ImportInvalidStockManaged = "item.import.invalid_stock_managed";
    public const string ImportInvalidNumber = "item.import.invalid_number";
    public const string ImportVendorUnknown = "item.import.vendor_unknown";
    public const string ImportNameRequired = "item.import.name_required";
    public const string ImportCannotClear = "item.import.cannot_clear";
    public const string AlreadyActive = "item.already_active";
    public const string AlreadyInactive = "item.already_inactive";
}
