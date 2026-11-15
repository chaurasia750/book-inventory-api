namespace BookInventory.Domain.Sales;

/// <summary>
/// Every message code of the <c>sales</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the human
/// text lives in <c>BookInventory.Application.Sales.SalesMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class SalesMessages
{
    public const string OrderNotFound = "sales.order_not_found";
    public const string InvoiceNotFound = "sales.invoice_not_found";
    public const string SchoolNotFound = "sales.school_not_found";
    public const string SchoolInactive = "sales.school_inactive";
    public const string ItemNotFound = "sales.item_not_found";
    public const string ItemInactive = "sales.item_inactive";
    public const string NoPrice = "sales.no_price";
    public const string InvalidPriceVersion = "sales.invalid_price_version";
    public const string InvalidQuantity = "sales.invalid_quantity";
    public const string InvalidDiscount = "sales.invalid_discount";
    public const string DiscountTooLarge = "sales.discount_too_large";
    public const string TooManyLines = "sales.too_many_lines";
    public const string InvalidDate = "sales.invalid_date";
    public const string InvalidSession = "sales.invalid_session";
    public const string InvalidText = "sales.invalid_text";
    public const string NotDraft = "sales.order_not_draft";
    public const string NoLines = "sales.order_no_lines";
    public const string NotConfirmed = "sales.order_not_confirmed";
    public const string InvalidReason = "sales.invalid_reason";
    public const string NothingToInvoice = "sales.nothing_to_invoice";
    public const string QuantityAboveRemaining = "sales.quantity_above_remaining";
    public const string TotalNotPositive = "sales.total_not_positive";
    public const string AlreadyCancelled = "sales.invoice_already_cancelled";
}
