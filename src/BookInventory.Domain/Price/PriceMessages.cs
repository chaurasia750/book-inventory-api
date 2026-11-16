namespace BookInventory.Domain.Price;

/// <summary>
/// Every message code of the <c>price</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the
/// human text lives in <c>BookInventory.Application.Price.PriceMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class PriceMessages
{
    public const string ItemNotFound = "price.item_not_found";
    public const string VersionNotFound = "price.version_not_found";
    public const string NameTaken = "price.name_taken";
    public const string InvalidName = "price.invalid_name";
    public const string InvalidAmount = "price.invalid_amount";
    public const string InvalidChange = "price.invalid_change";
    public const string NothingSelected = "price.nothing_selected";
    public const string TooManyItems = "price.too_many_items";
}
