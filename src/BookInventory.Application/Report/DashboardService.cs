using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Report;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BookInventory.Application.Report;

/// <summary>
/// R1-R3, R13: builds the dashboard. The session label is checked (R3); the parts the caller may see (R2) are read one after the
/// other through the readers (the request's database context is not thread safe) and each is guarded on its own: a part that fails
/// shows <c>report.part_failed</c> while the others still show. Nothing is stored; every call reads again (R1).
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DashboardService(
    BusinessCalendar calendar, DashboardPaymentsReader payments, DashboardSalesReader sales, DashboardOwedReader owed, DashboardStockReader stock, ILogger<DashboardService> logger)
{
    public async Task<DashboardResponse> GetAsync(DashboardQuery query, CancellationToken cancellationToken)
    {
        var today = calendar.Today();
        var session = string.IsNullOrWhiteSpace(query.Session) ? DashboardSession.Current(today) : DashboardSession.Parse(query.Session);
        var window = new DashboardWindow(session, today);
        var choices = DashboardSession.ChoicesAt(today).Select((s, i) => new SessionChoice(s.Label, i == 0)).ToList();

        return new DashboardResponse(
            session.Label, window.From, window.To, today, choices,
            query.SeesMoney ? await GuardAsync("payments", () => payments.ReadAsync(window, cancellationToken)) : null,
            await GuardAsync("orders", () => sales.OrdersAsync(window, cancellationToken)),
            await GuardAsync("sales", () => sales.SalesAsync(window, cancellationToken)),
            query.SeesMoney ? await GuardAsync("returns", () => sales.ReturnsAsync(window, cancellationToken)) : null,
            query.SeesMoney ? await GuardAsync("owed", () => owed.ReadAsync(cancellationToken)) : null,
            await GuardAsync("stock", () => stock.ReadAsync(window, query.SeesMoney, cancellationToken)));
    }

    /// <summary>R13: one part on its own; a cancelled request is not a failed part.</summary>
    private async Task<DashboardPart<T>> GuardAsync<T>(string name, Func<Task<T>> read) where T : class
    {
        try
        {
            return new DashboardPart<T>(await read());
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "The dashboard part {Part} could not be read", name);
            return new DashboardPart<T>(null, ReportMessages.PartFailed);
        }
    }
}
