using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>R7: the orders of one status: how many and their grand total.</summary>
public sealed record OrderStatusFigure(SchoolOrderStatus Status, int Count, decimal Total);

/// <summary>R7: the orders and invoices of a session: by status, the books still to invoice, the live and cancelled invoices.</summary>
public sealed record OrderFigures(IReadOnlyList<OrderStatusFigure> ByStatus, int StillToInvoiceQuantity, int InvoicesLive, decimal InvoicesTotal, int InvoicesCancelled);

/// <summary>R8: what was sold in a date range: the live school invoices and the live counter bills.</summary>
public sealed record SalesFigure(int Invoices, decimal InvoicesTotal, int CounterBills, decimal CounterTotal)
{
    public decimal Total => InvoicesTotal + CounterTotal;
}

/// <summary>R8: one day's sale (invoices plus counter bills).</summary>
public sealed record DayFigure(DateOnly Date, decimal Total);

/// <summary>R8: the live credit notes and refunds of a date range.</summary>
public sealed record ReturnFigure(int CreditNotes, decimal CreditNotesTotal, int Refunds, decimal RefundsTotal);

/// <summary>R6: a live invoice of an earlier session: its id, its school and its grand total.</summary>
public sealed record EarlierInvoice(Guid Id, Guid SchoolId, decimal GrandTotal);

/// <summary>
/// R7, R8, R6: the capability the report module uses to read the figures of the dashboard, never by joining the sales tables. It
/// answers only for the session's company, returns plain values, never counts a cancelled document as sold, and changes nothing.
/// </summary>
public interface ISalesFigures
{
    /// <summary>R7: the orders whose session starts in <paramref name="startYear"/> and their invoices.</summary>
    Task<OrderFigures> OrdersAsync(int startYear, CancellationToken cancellationToken = default);

    /// <summary>R8: the live invoices and counter bills dated from <paramref name="from"/> to <paramref name="to"/>, both included.</summary>
    Task<SalesFigure> SalesAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>R8: the sale of each day of the range that had one, oldest first.</summary>
    Task<IReadOnlyList<DayFigure>> DaysAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>R8: the live credit notes and refunds dated in the range.</summary>
    Task<ReturnFigure> ReturnsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>R6: the live invoices whose session starts before <paramref name="startYear"/>.</summary>
    Task<IReadOnlyList<EarlierInvoice>> EarlierSessionInvoicesAsync(int startYear, CancellationToken cancellationToken = default);
}
