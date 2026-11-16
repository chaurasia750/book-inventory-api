using BookInventory.Application.Purchase;
using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Report;

/// <summary>R7, R8: the orders and invoices, sales and returns cards of the dashboard, read through the sales and purchase figures capabilities.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DashboardSalesReader(ISalesFigures sales, IPurchaseFigures purchases)
{
    /// <summary>R8 (Q4): the line is the last 30 days of the window, by day.</summary>
    private const int DaysShown = 30;

    private static readonly SalesFigure NoSales = new(0, 0m, 0, 0m);

    /// <summary>R7: the orders whose session starts in the dashboard session's start year, whatever the window.</summary>
    public async Task<OrdersPart> OrdersAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        var o = await sales.OrdersAsync(window.Session.StartYear, cancellationToken);
        return new OrdersPart(o.ByStatus, o.StillToInvoiceQuantity, o.InvoicesLive, o.InvoicesTotal, o.InvoicesCancelled);
    }

    /// <summary>R8: today's sale, the session's sale and the last days by day; a session that has not started shows zeros (today's sale is still today's).</summary>
    public async Task<SalesPart> SalesAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        var today = await sales.SalesAsync(window.Today, window.Today, cancellationToken);
        if (!window.Started)
            return new SalesPart(today, NoSales, []);

        var daysFrom = window.To.AddDays(1 - DaysShown) > window.From ? window.To.AddDays(1 - DaysShown) : window.From;
        return new SalesPart(today, await sales.SalesAsync(window.From, window.To, cancellationToken), await sales.DaysAsync(daysFrom, window.To, cancellationToken));
    }

    /// <summary>R8: the live credit notes, refunds and vendor returns dated in the window.</summary>
    public async Task<ReturnsPart> ReturnsAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        if (!window.Started)
            return new ReturnsPart(0, 0m, 0, 0m, 0, 0m);

        var returned = await sales.ReturnsAsync(window.From, window.To, cancellationToken);
        var vendor = await purchases.ReturnsAsync(window.From, window.To, cancellationToken);
        return new ReturnsPart(returned.CreditNotes, returned.CreditNotesTotal, returned.Refunds, returned.RefundsTotal, vendor.Count, vendor.Total);
    }
}
