using System.Globalization;
using BookInventory.Application.Sales;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Sales;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Sales;

/// <summary>The data side of the dashboard figures of sales (spec report R6-R8): grouped queries limited to the session's company by the platform filter.</summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(ISalesFigures))]
public sealed class SalesFigures(IUnitOfWork unitOfWork) : ISalesFigures
{
    private readonly IRepository<SchoolOrder> _orders = unitOfWork.Repository<SchoolOrder>();
    private readonly IRepository<Invoice> _invoices = unitOfWork.Repository<Invoice>();
    private readonly IRepository<CounterBill> _bills = unitOfWork.Repository<CounterBill>();
    private readonly IRepository<CreditNote> _notes = unitOfWork.Repository<CreditNote>();
    private readonly IRepository<SchoolRefund> _refunds = unitOfWork.Repository<SchoolRefund>();

    public async Task<OrderFigures> OrdersAsync(int startYear, CancellationToken cancellationToken = default)
    {
        var prefix = startYear.ToString(CultureInfo.InvariantCulture);
        var byStatus = await _orders.TableNoTracking
            .Where(o => o.Session.StartsWith(prefix))
            .GroupBy(o => o.Status)
            .Select(g => new OrderStatusFigure(g.Key, g.Count(), g.Sum(o => o.Totals.GrandTotal)))
            .ToListAsync(cancellationToken);
        var remaining = await _orders.TableNoTracking
            .Where(o => o.Session.StartsWith(prefix) && o.Status == SchoolOrderStatus.Confirmed)
            .SelectMany(o => o.Lines)
            .SumAsync(l => (int?)(l.Quantity - l.InvoicedQuantity), cancellationToken) ?? 0;
        var invoices = await _invoices.TableNoTracking
            .Where(i => i.Session.StartsWith(prefix))
            .GroupBy(i => i.Status)
            .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(i => i.Totals.GrandTotal) })
            .ToListAsync(cancellationToken);

        var live = invoices.FirstOrDefault(i => i.Status == InvoiceStatus.Issued);
        var cancelled = invoices.FirstOrDefault(i => i.Status == InvoiceStatus.Cancelled);
        return new OrderFigures(
            [.. Enum.GetValues<SchoolOrderStatus>().Select(s => byStatus.FirstOrDefault(b => b.Status == s) ?? new OrderStatusFigure(s, 0, 0m))],
            remaining, live?.Count ?? 0, live?.Total ?? 0m, cancelled?.Count ?? 0);
    }

    public async Task<SalesFigure> SalesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoices.TableNoTracking
            .Where(i => i.Status == InvoiceStatus.Issued && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(i => i.Totals.GrandTotal) })
            .FirstOrDefaultAsync(cancellationToken);
        var bills = await _bills.TableNoTracking
            .Where(b => b.Status == CounterBillStatus.Active && b.BillDate >= from && b.BillDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(b => b.Totals.GrandTotal) })
            .FirstOrDefaultAsync(cancellationToken);
        return new SalesFigure(invoices?.Count ?? 0, invoices?.Total ?? 0m, bills?.Count ?? 0, bills?.Total ?? 0m);
    }

    public async Task<IReadOnlyList<DayFigure>> DaysAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var invoices = await _invoices.TableNoTracking
            .Where(i => i.Status == InvoiceStatus.Issued && i.InvoiceDate >= from && i.InvoiceDate <= to)
            .GroupBy(i => i.InvoiceDate)
            .Select(g => new DayFigure(g.Key, g.Sum(i => i.Totals.GrandTotal)))
            .ToListAsync(cancellationToken);
        var bills = await _bills.TableNoTracking
            .Where(b => b.Status == CounterBillStatus.Active && b.BillDate >= from && b.BillDate <= to)
            .GroupBy(b => b.BillDate)
            .Select(g => new DayFigure(g.Key, g.Sum(b => b.Totals.GrandTotal)))
            .ToListAsync(cancellationToken);
        return [.. invoices.Concat(bills).GroupBy(d => d.Date).Select(g => new DayFigure(g.Key, g.Sum(d => d.Total))).OrderBy(d => d.Date)];
    }

    public async Task<ReturnFigure> ReturnsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var notes = await _notes.TableNoTracking
            .Where(n => n.Status == CreditNoteStatus.Active && n.NoteDate >= from && n.NoteDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(n => n.Totals.GrandTotal) })
            .FirstOrDefaultAsync(cancellationToken);
        var refunds = await _refunds.TableNoTracking
            .Where(r => r.Status == RefundStatus.Active && r.RefundDate >= from && r.RefundDate <= to)
            .GroupBy(_ => 1)
            .Select(g => new { Count = g.Count(), Total = g.Sum(r => r.Amount) })
            .FirstOrDefaultAsync(cancellationToken);
        return new ReturnFigure(notes?.Count ?? 0, notes?.Total ?? 0m, refunds?.Count ?? 0, refunds?.Total ?? 0m);
    }

    public async Task<IReadOnlyList<EarlierInvoice>> EarlierSessionInvoicesAsync(int startYear, CancellationToken cancellationToken = default)
    {
        var prefix = startYear.ToString(CultureInfo.InvariantCulture);
        return await _invoices.TableNoTracking
            .Where(i => i.Status == InvoiceStatus.Issued && i.Session.CompareTo(prefix) < 0)
            .Select(i => new EarlierInvoice(i.Id, i.SchoolId, i.Totals.GrandTotal))
            .ToListAsync(cancellationToken);
    }
}
