namespace BookInventory.Domain.Vendor;

/// <summary>
/// Every message code of the <c>vendor</c> module (spec R55 of iam applies to all modules). The domain throws these codes;
/// the human text lives in <c>BookInventory.Application.Vendor.VendorMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class VendorMessages
{
    public const string NotFound = "vendor.not_found";
    public const string NameTaken = "vendor.name_taken";
    public const string InvalidName = "vendor.invalid_name";
    public const string InvalidField = "vendor.invalid_field";
    public const string InvalidState = "vendor.invalid_state";
    public const string InvalidMobile = "vendor.invalid_mobile";
    public const string InvalidEmail = "vendor.invalid_email";
    public const string InvalidGstin = "vendor.invalid_gstin";
    public const string InvalidBank = "vendor.invalid_bank";
    public const string InvalidOpeningBalance = "vendor.invalid_opening_balance";
    public const string AlreadyActive = "vendor.already_active";
    public const string AlreadyInactive = "vendor.already_inactive";
}
