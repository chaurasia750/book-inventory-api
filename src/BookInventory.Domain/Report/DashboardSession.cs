using System.Globalization;
using BookInventory.BuildingBlocks.Domain.Exceptions;

namespace BookInventory.Domain.Report;

/// <summary>
/// Spec report R3, D2: the session chosen on the dashboard: a label <c>2026-27</c>, the April-March year it starts in. Its window is
/// 1 April of that year to the earlier of today and 31 March of the next; a session that has not started has no window. The
/// <see cref="StartYear"/> joins the schools of other session shapes (school R18: a Jan-Dec school's <c>2026</c> starts in 2026 too).
/// </summary>
public sealed record DashboardSession
{
    /// <summary>R3 (Q3): the session dropdown offers the current session and the four before it.</summary>
    public const int Choices = 5;

    private DashboardSession(int startYear) => StartYear = startYear;

    public int StartYear { get; }

    /// <summary>The label, for example <c>2026-27</c>.</summary>
    public string Label => string.Create(CultureInfo.InvariantCulture, $"{StartYear}-{(StartYear + 1) % 100:D2}");

    public DateOnly Start => new(StartYear, 4, 1);

    public DateOnly End => new(StartYear + 1, 3, 31);

    /// <summary>R3: the session whose April-March year contains <paramref name="today"/>.</summary>
    public static DashboardSession Current(DateOnly today) => new(today.Month >= 4 ? today.Year : today.Year - 1);

    /// <summary>R3: the label <c>YYYY-YY</c> with the second part the next year, or <c>400 report.invalid_session</c>.</summary>
    public static DashboardSession Parse(string label)
    {
        var text = label?.Trim() ?? string.Empty;
        var valid = text.Length == 7 && text[4] == '-'
            && int.TryParse(text.AsSpan(0, 4), NumberStyles.None, CultureInfo.InvariantCulture, out var year)
            && int.TryParse(text.AsSpan(5, 2), NumberStyles.None, CultureInfo.InvariantCulture, out var next)
            && year is >= 2000 and <= 2999 && next == (year + 1) % 100;
        return valid ? new DashboardSession(int.Parse(text.AsSpan(0, 4), CultureInfo.InvariantCulture)) : throw new ValidationFailureException(ReportMessages.InvalidSession);
    }

    /// <summary>R3: the sessions the screen offers, newest first: the current one and the four before it.</summary>
    public static IReadOnlyList<DashboardSession> ChoicesAt(DateOnly today) =>
        [.. Enumerable.Range(0, Choices).Select(i => new DashboardSession(Current(today).StartYear - i))];

    /// <summary>R3: the last day the window covers on <paramref name="today"/>; before <see cref="Start"/> the window is empty (<see cref="HasStarted"/> is false).</summary>
    public DateOnly WindowEnd(DateOnly today) => today < End ? today : End;

    public bool HasStarted(DateOnly today) => today >= Start;
}
