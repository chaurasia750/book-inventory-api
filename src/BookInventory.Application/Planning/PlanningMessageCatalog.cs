using BookInventory.Domain.Planning;

namespace BookInventory.Application.Planning;

/// <summary>The human text of every <see cref="PlanningMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class PlanningMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [PlanningMessages.SchoolNotFound] = "The school could not be found.",
            [PlanningMessages.ListNotFound] = "This class has no book list for the session.",
            [PlanningMessages.InvalidClass] = "Choose one of the school's classes.",
            [PlanningMessages.InvalidSession] = "Write a session the way the school counts them (2026-27 or 2027).",
            [PlanningMessages.ItemNotFound] = "Item not found.",
            [PlanningMessages.ItemInactive] = "The item is inactive.",
            [PlanningMessages.InvalidQuantity] = "A quantity per student is a whole number from 1 to 50.",
            [PlanningMessages.InvalidEstimate] = "The number of students is a whole number from 0 to 5000.",
            [PlanningMessages.TooManyLines] = "A class book list has at most 200 books.",
            [PlanningMessages.SameSource] = "Copy from another session or another school."
        };
}
