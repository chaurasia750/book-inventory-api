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

    /// <summary>R18: the type a session label is written in (<c>2026-27</c> Apr-Mar, <c>2027</c> Jan-Dec), or <c>null</c> for any other text.</summary>
    public static SessionType? TypeOf(string? label) => Parse(label)?.Type;

    /// <summary>R18: the label of the session before <paramref name="label"/>, or <c>null</c> when it is not a session label.</summary>
    public static string? PreviousOf(string? label) => Parse(label) is { } p ? Label(p.Type, p.Start - 1) : null;

    private static (SessionType Type, int Start)? Parse(string? label)
    {
        var text = label?.Trim() ?? string.Empty;
        if (text.Length == 4 && int.TryParse(text, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var year))
            return year is >= 2000 and <= 2100 ? (SessionType.JanDec, year) : null;

        if (text.Length != 7 || text[4] != '-'
            || !int.TryParse(text[..4], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var start)
            || !int.TryParse(text[5..], System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var end))
            return null;

        return start is >= 2000 and <= 2100 && end == (start + 1) % 100 ? (SessionType.AprMar, start) : null;
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
