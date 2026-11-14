using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.School;

/// <summary>
/// R2/D6: the states a school can be in. The business runs only in Bihar for now, so the list holds Bihar and a new school
/// starts there; a further state is one more entry here (and the CGST/SGST against IGST rule of BR-GST-05 then matters).
/// </summary>
public static class IndianState
{
    public const string Default = "Bihar";

    public static readonly IReadOnlyList<string> All = [Default];

    /// <summary>The stored spelling of a state, or the default when none is given; anything outside the list is refused.</summary>
    public static string Require(string? value)
    {
        var trimmed = value?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return Default;

        return All.FirstOrDefault(s => s.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            ?? throw new ValidationFailureException(SchoolMessages.InvalidState);
    }
}
