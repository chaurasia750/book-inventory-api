namespace BookInventory.Domain.School;

/// <summary>
/// R18 (BR-SES-04, BR-SES-05): the one rule that tells which session a date belongs to. An Apr-Mar session is named by its April
/// year (<c>2027-28</c>): a date from the cut-off to 31 March belongs to the session starting that April. A Jan-Dec session is
/// named by its year (<c>2027</c>): a date from the cut-off to 31 December belongs to the next year's session.
/// </summary>
public static class SchoolSession
{
    public static SessionChoice Of(SessionType type, DateOnly date, SessionCutOff cutOff)
    {
        var start = type == SessionType.AprMar ? AprMarStart(date, cutOff) : JanDecYear(date, cutOff);
        return new SessionChoice(Label(type, start), Label(type, start - 1), Label(type, start + 1));
    }

    private static int AprMarStart(DateOnly date, SessionCutOff cutOff)
    {
        if (date.Month >= 4)
            return date.Year;

        return cutOff.IsOnOrBefore(date) ? date.Year : date.Year - 1;
    }

    private static int JanDecYear(DateOnly date, SessionCutOff cutOff) => cutOff.IsOnOrBefore(date) ? date.Year + 1 : date.Year;

    private static string Label(SessionType type, int start) =>
        type == SessionType.AprMar ? $"{start}-{(start + 1) % 100:D2}" : start.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
