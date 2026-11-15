using BookInventory.Domain.Item;

namespace BookInventory.Application.Item;

/// <summary>The human text of every <see cref="ItemMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class ItemMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ItemMessages.NotFound] = "The item could not be found.",
            [ItemMessages.InvalidName] = "The item name must be 2 to 200 characters.",
            [ItemMessages.InvalidField] = "An item detail is too long.",
            [ItemMessages.InvalidHsn] = "The HSN code must have 4 to 8 digits.",
            [ItemMessages.InvalidGstRate] = "The GST rate must be 0, 5, 12, 18 or 28 percent.",
            [ItemMessages.InvalidVendor] = "The vendor must be an active vendor of your company.",
            [ItemMessages.InvalidBarcode] = "A barcode must have 4 to 30 letters, digits or hyphens.",
            [ItemMessages.DuplicateBarcode] = "A barcode appears twice in the list.",
            [ItemMessages.BarcodeTaken] = "A barcode is already used by another item.",
            [ItemMessages.ImportInvalidFile] = "Upload an Excel file (.xlsx) made from the template.",
            [ItemMessages.ImportTooLarge] = "The file is larger than 5 MB.",
            [ItemMessages.ImportTooManyRows] = "The file has more than 5000 rows.",
            [ItemMessages.ImportEmpty] = "The file has no rows to import.",
            [ItemMessages.ImportRowsInvalid] = "Some rows of the file are wrong; nothing was imported. Fix the rows listed and upload again.",
            [ItemMessages.ImportItemUnknown] = "No item of your company has this item code.",
            [ItemMessages.ImportDuplicateRow] = "This item or barcode appears more than once in the file.",
            [ItemMessages.ImportInvalidCategory] = "The category must be Book, Copy, Pen, Pencil, Bag, Tie or Other.",
            [ItemMessages.ImportInvalidStockManaged] = "Stock managed must be yes or no.",
            [ItemMessages.ImportInvalidNumber] = "The GST rate must be a number.",
            [ItemMessages.ImportVendorUnknown] = "No active vendor of your company has this name.",
            [ItemMessages.ImportNameRequired] = "A new item needs a name.",
            [ItemMessages.ImportCannotClear] = "The name and the category cannot be cleared.",
            [ItemMessages.AlreadyActive] = "The item is already active.",
            [ItemMessages.AlreadyInactive] = "The item is already inactive."
        };
}
