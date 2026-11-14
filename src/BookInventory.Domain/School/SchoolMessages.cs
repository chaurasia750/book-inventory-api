namespace BookInventory.Domain.School;

/// <summary>
/// Every message code of the <c>school</c> module (spec R55 of iam applies to all modules). The domain throws these codes;
/// the human text lives in <c>BookInventory.Application.School.SchoolMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class SchoolMessages
{
    public const string NotFound = "school.not_found";
    public const string CodeTaken = "school.code_taken";
    public const string InvalidName = "school.invalid_name";
    public const string InvalidField = "school.invalid_field";
    public const string InvalidState = "school.invalid_state";
    public const string InvalidMobile = "school.invalid_mobile";
    public const string InvalidEmail = "school.invalid_email";
    public const string InvalidOpeningBalance = "school.invalid_opening_balance";
    public const string InvalidClassName = "school.invalid_class_name";
    public const string DuplicateClass = "school.duplicate_class";
    public const string AlreadyActive = "school.already_active";
    public const string AlreadyInactive = "school.already_inactive";
}
