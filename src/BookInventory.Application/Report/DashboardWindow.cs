using BookInventory.Domain.Report;

namespace BookInventory.Application.Report;

/// <summary>
/// R3: what a reader of the dashboard is asked about: the session, today and the window (the session's first day to the earlier of
/// today and its last day). A session that has not started has no window: <see cref="Started"/> is false and every figure is zero.
/// </summary>
public sealed record DashboardWindow(DashboardSession Session, DateOnly Today)
{
    public bool Started => Session.HasStarted(Today);

    public DateOnly From => Session.Start;

    public DateOnly To => Session.WindowEnd(Today);
}
