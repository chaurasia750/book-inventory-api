namespace BookInventory.Domain.Planning;

/// <summary>
/// Every message code of the <c>planning</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the
/// human text lives in <c>BookInventory.Application.Planning.PlanningMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class PlanningMessages
{
    public const string SchoolNotFound = "planning.school_not_found";
    public const string ListNotFound = "planning.list_not_found";
    public const string InvalidClass = "planning.invalid_class";
    public const string InvalidSession = "planning.invalid_session";
    public const string ItemNotFound = "planning.item_not_found";
    public const string ItemInactive = "planning.item_inactive";
    public const string InvalidQuantity = "planning.invalid_quantity";
    public const string InvalidEstimate = "planning.invalid_estimate";
    public const string TooManyLines = "planning.too_many_lines";
    public const string SameSource = "planning.copy_same_source";
}
