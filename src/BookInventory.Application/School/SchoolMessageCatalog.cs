using BookInventory.Domain.School;

namespace BookInventory.Application.School;

/// <summary>The human text of every <see cref="SchoolMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class SchoolMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [SchoolMessages.NotFound] = "The school could not be found.",
            [SchoolMessages.CodeTaken] = "A school with this code already exists.",
            [SchoolMessages.InvalidName] = "The school name must be 2 to 150 characters.",
            [SchoolMessages.InvalidField] = "A school detail is too long.",
            [SchoolMessages.InvalidState] = "The state is not one of the states the business works in.",
            [SchoolMessages.InvalidMobile] = "The mobile number must have 10 to 15 digits.",
            [SchoolMessages.InvalidEmail] = "The email address is not valid.",
            [SchoolMessages.InvalidOpeningBalance] = "The opening balance must be zero or more, with at most two decimals.",
            [SchoolMessages.InvalidClassName] = "A class name must be 1 to 50 characters.",
            [SchoolMessages.DuplicateClass] = "A class name appears twice in the list.",
            [SchoolMessages.AlreadyActive] = "The school is already active.",
            [SchoolMessages.AlreadyInactive] = "The school is already inactive."
        };
}
