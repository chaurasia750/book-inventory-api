using System.Globalization;
using BookInventory.Application.Payment;
using BookInventory.Application.Price;
using BookInventory.BuildingBlocks.Application.Attributes;
using BookInventory.BuildingBlocks.Persistence.Contracts;
using BookInventory.Domain.Payment;
using Microsoft.EntityFrameworkCore;

namespace BookInventory.Infrastructure.Payment;

/// <summary>
/// The data side of the dashboard figures of payments (spec report R4-R6): grouped queries limited to the session's company by the
/// platform filter. A row of an allocation keeps no date column, but its id is a UUID v7 whose first 48 bits are the moment it was
/// made (CLAUDE.project.md "Identifiers"), so the business date of a row (Q2) is read from its id.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped, typeof(IPaymentFigures))]
public sealed class PaymentFigures(IUnitOfWork unitOfWork, BusinessCalendar calendar) : IPaymentFigures
{
    private readonly IRepository<Receipt> _receipts = unitOfWork.Repository<Receipt>();

    public async Task<ReceivedFigure> ReceivedAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var recorded = _receipts.TableNoTracking.Where(r => r.Status == ReceiptStatus.Recorded);
        var count = await recorded.CountAsync(r => r.ReceiptDate >= from && r.ReceiptDate <= to, cancellationToken);
        var modes = recorded.SelectMany(r => r.Modes.Select(m => new { r.ReceiptDate, m.ModeType, m.Amount, m.ChequeStatus, m.ResultDate }));

        var plain = await modes
            .Where(m => m.ModeType != ModeType.Cheque && m.ReceiptDate >= from && m.ReceiptDate <= to)
            .GroupBy(m => m.ModeType)
            .Select(g => new ModeFigure(g.Key, g.Sum(m => m.Amount)))
            .ToListAsync(cancellationToken);
        var cheques = await modes
            .Where(m => m.ModeType == ModeType.Cheque && m.ChequeStatus == ChequeStatus.Cleared && m.ResultDate >= from && m.ResultDate <= to)
            .SumAsync(m => (decimal?)m.Amount, cancellationToken) ?? 0m;

        return new ReceivedFigure(
            [.. Enum.GetValues<ModeType>().Select(t => t == ModeType.Cheque ? new ModeFigure(t, cheques) : plain.FirstOrDefault(p => p.Mode == t) ?? new ModeFigure(t, 0m))],
            count);
    }

    public async Task<PendingChequeFigure> PendingChequesAsync(CancellationToken cancellationToken = default)
    {
        var pending = await _receipts.TableNoTracking
            .Where(r => r.Status == ReceiptStatus.Recorded)
            .SelectMany(r => r.Modes)
            .Where(m => m.ModeType == ModeType.Cheque && m.ChequeStatus == ChequeStatus.Pending)
            .GroupBy(_ => 1)
            .Select(g => new PendingChequeFigure(g.Count(), g.Sum(m => m.Amount)))
            .FirstOrDefaultAsync(cancellationToken);
        return pending ?? new PendingChequeFigure(0, 0m);
    }

    public async Task<IReadOnlyList<InvoiceAllocation>> AllocationsAsync(IReadOnlyCollection<Guid> invoiceIds, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        if (invoiceIds.Count == 0)
            return [];

        var ids = invoiceIds.ToList();
        var rows = await _receipts.TableNoTracking
            .SelectMany(r => r.Allocations)
            .Where(a => ids.Contains(a.InvoiceId))
            .Select(a => new { a.Id, a.InvoiceId, a.Amount })
            .ToListAsync(cancellationToken);

        return [.. rows
            .GroupBy(a => a.InvoiceId)
            .Select(g => new InvoiceAllocation(g.Key, g.Where(a => IsMadeIn(a.Id, from, to)).Sum(a => a.Amount), g.Sum(a => a.Amount)))];
    }

    /// <summary>The business date a UUID v7 id was made on: its first 48 bits are Unix milliseconds.</summary>
    private bool IsMadeIn(Guid id, DateOnly from, DateOnly to)
    {
        var milliseconds = long.Parse(id.ToString("N")[..12], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var date = calendar.DateOf(DateTimeOffset.FromUnixTimeMilliseconds(milliseconds));
        return date >= from && date <= to;
    }
}
