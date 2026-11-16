namespace BookInventory.Domain.Purchase;

/// <summary>
/// Message codes of the purchase module (spec purchase R1-R13). The human text lives in
/// <c>BookInventory.Application.Purchase.PurchaseMessageCatalog</c>; the codes are stable API surface.
/// </summary>
public static class PurchaseMessages
{
    public const string VendorReturnNotFound = "purchase.vendor_return.not_found";
    public const string VendorNotFound = "purchase.vendor_return.vendor_not_found";
    public const string VendorInactive = "purchase.vendor_return.vendor_inactive";
    public const string ItemNotFound = "purchase.vendor_return.item_not_found";
    public const string ItemInactive = "purchase.vendor_return.item_inactive";
    public const string NoPurchasePrice = "purchase.vendor_return.no_purchase_price";
    public const string InvalidDate = "purchase.vendor_return.invalid_date";
    public const string NoLines = "purchase.vendor_return.no_lines";
    public const string TooManyLines = "purchase.vendor_return.too_many_lines";
    public const string InvalidRate = "purchase.vendor_return.invalid_rate";
    public const string InvalidQuantity = "purchase.vendor_return.invalid_quantity";
    public const string InvalidSource = "purchase.vendor_return.invalid_source";
    public const string InvalidText = "purchase.vendor_return.invalid_text";
    public const string InvalidReason = "purchase.vendor_return.invalid_reason";
    public const string TotalNotPositive = "purchase.vendor_return.total_not_positive";
    public const string AlreadyCancelled = "purchase.vendor_return.already_cancelled";
}
