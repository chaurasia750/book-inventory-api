using BookInventory.Application.Payment;
using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.Domain.Payment;
using Microsoft.Extensions.DependencyInjection;

namespace BookInventory.Application.Report;

/// <summary>R4-R6: the payments card of the dashboard (the Owner's), read through the payment and sales figures capabilities.</summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class DashboardPaymentsReader(IPaymentFigures payments, ISalesFigures sales)
{
    public async Task<PaymentsPart> ReadAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        var pending = await payments.PendingChequesAsync(cancellationToken);
        if (!window.Started)
            return new PaymentsPart(new ReceivedPart(0m, [.. Enum.GetValues<ModeType>().Select(m => new ModeFigure(m, 0m))], 0), pending, new EarlierSessionsPart(0m, 0, 0m));

        var received = await payments.ReceivedAsync(window.From, window.To, cancellationToken);
        return new PaymentsPart(new ReceivedPart(received.Total, received.ByMode, received.Receipts), pending, await EarlierAsync(window, cancellationToken));
    }

    /// <summary>R6 (D4, Q2): the net allocated inside the window to the invoices of earlier sessions, their schools and what is still due on them today.</summary>
    private async Task<EarlierSessionsPart> EarlierAsync(DashboardWindow window, CancellationToken cancellationToken)
    {
        var invoices = await sales.EarlierSessionInvoicesAsync(window.Session.StartYear, cancellationToken);
        var allocations = (await payments.AllocationsAsync([.. invoices.Select(i => i.Id)], window.From, window.To, cancellationToken)).ToDictionary(a => a.InvoiceId);

        var schools = invoices.Where(i => allocations.TryGetValue(i.Id, out var a) && a.NetInWindow != 0m).Select(i => i.SchoolId).Distinct().Count();
        var stillDue = invoices.Sum(i => Math.Max(0m, i.GrandTotal - (allocations.TryGetValue(i.Id, out var a) ? a.AllocatedTotal : 0m)));
        return new EarlierSessionsPart(allocations.Values.Sum(a => a.NetInWindow), schools, stillDue);
    }
}
