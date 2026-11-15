namespace BookInventory.Domain.Customer;

/// <summary>
/// Every message code of the <c>customer</c> module (spec R55 of iam applies to all modules). The domain throws these codes;
/// the human text lives in <c>BookInventory.Application.Customer.CustomerMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class CustomerMessages
{
    public const string NotFound = "customer.not_found";
    public const string InvalidName = "customer.invalid_name";
    public const string InvalidField = "customer.invalid_field";
    public const string InvalidMobile = "customer.invalid_mobile";
    public const string MobileRequired = "customer.mobile_required";
    public const string MobileTaken = "customer.mobile_taken";
    public const string InvalidSchool = "customer.invalid_school";
    public const string InvalidClass = "customer.invalid_class";
    public const string InvalidOpeningBalance = "customer.invalid_opening_balance";
    public const string InvalidCreditLimit = "customer.invalid_credit_limit";
    public const string AlreadyActive = "customer.already_active";
    public const string AlreadyInactive = "customer.already_inactive";
}
