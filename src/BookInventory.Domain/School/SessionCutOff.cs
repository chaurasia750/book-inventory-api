using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.School;

/// <summary>
/// R19: a cut-off day: from this day and month on, a date belongs to the next session. An Apr-Mar cut-off lies from 1 January to
/// 31 March (the months before the session starts), a Jan-Dec cut-off from 1 October to 31 December; the day must exist in its
/// month in every year (29 February is refused).
/// </summary>
public sealed record SessionCutOff(int Month, int Day)
{
    public static readonly SessionCutOff AprMarDefault = new(2, 1);
    public static readonly SessionCutOff JanDecDefault = new(11, 15);

    /// <summary>R19: a valid cut-off for the session type, or <c>400 school.invalid_cut_off</c>.</summary>
    public static SessionCutOff Require(SessionType type, int month, int day)
    {
        var (first, last) = type == SessionType.AprMar ? (1, 3) : (10, 12);
        if (month < first || month > last || day < 1 || day > DateTime.DaysInMonth(2027, month))
            throw new ValidationFailureException(SchoolMessages.InvalidCutOff);

        return new SessionCutOff(month, day);
    }

    public bool IsOnOrBefore(DateOnly date) => date.Month > Month || (date.Month == Month && date.Day >= Day);
}
