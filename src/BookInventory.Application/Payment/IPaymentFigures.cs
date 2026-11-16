using BookInventory.Domain.Payment;

namespace BookInventory.Application.Payment;

/// <summary>R4: money received in one way of payment.</summary>
public sealed record ModeFigure(ModeType Mode, decimal Amount);

/// <summary>R4: what was received in a date range by way of payment and how many receipts were recorded.</summary>
public sealed record ReceivedFigure(IReadOnlyList<ModeFigure> ByMode, int Receipts)
{
    public decimal Total => ByMode.Sum(m => m.Amount);
}

/// <summary>R5: the cheques still pending: how many and their amount.</summary>
public sealed record PendingChequeFigure(int Count, decimal Amount);

/// <summary>R6: what was allocated to one invoice inside a window (net of reductions) and in all.</summary>
public sealed record InvoiceAllocation(Guid InvoiceId, decimal NetInWindow, decimal AllocatedTotal);

/// <summary>
/// R4-R6: the capability the report module uses to read the payment figures of the dashboard, never by joining the payment tables.
/// It answers only for the session's company, returns plain values and changes nothing. A cancelled receipt and a bounced cheque
/// count for nothing received.
/// </summary>
public interface IPaymentFigures
{
    /// <summary>R4: Cash, UPI and Bank transfer by receipt date and cheques by clearing date, from <paramref name="from"/> to <paramref name="to"/> included.</summary>
    Task<ReceivedFigure> ReceivedAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>R5: the cheques of recorded receipts that are still pending, of any date.</summary>
    Task<PendingChequeFigure> PendingChequesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// R6, Q2: for each of the invoices, what was allocated to it inside the window (the signed rows made on a business date from
    /// <paramref name="from"/> to <paramref name="to"/>, a reduction being negative) and in all; an invoice with nothing allocated is left out.
    /// </summary>
    Task<IReadOnlyList<InvoiceAllocation>> AllocationsAsync(IReadOnlyCollection<Guid> invoiceIds, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
