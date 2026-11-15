using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// The persistence port of the <c>sales</c> module. Every read is already limited to the session's company by the platform filter
/// (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ISalesStore
{
    Task AddAsync(SchoolOrder order, CancellationToken cancellationToken = default);

    void Remove(SchoolOrder order);

    /// <summary>The order about to change, tracked, with its lines; <c>null</c> for an unknown id or another company's.</summary>
    Task<SchoolOrder?> FindOrderForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R6: the order about to be deleted, tracked, without its lines (a root is removed on its own, as the planning module does); <c>null</c> for an unknown id or another company's.</summary>
    Task<SchoolOrder?> FindOrderForRemovalAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The order for reading, not tracked, with its lines.</summary>
    Task<SchoolOrder?> FindOrderForReadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Invoice invoice, CancellationToken cancellationToken = default);

    /// <summary>The invoice about to be cancelled, tracked, with its lines.</summary>
    Task<Invoice?> FindInvoiceForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The invoice for reading, not tracked, with its lines.</summary>
    Task<Invoice?> FindInvoiceForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R15: the invoices of an order, newest first.</summary>
    Task<IReadOnlyList<InvoiceSummary>> InvoicesOfOrderAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>R15: the paged, searchable, filterable invoice list, newest first.</summary>
    Task<IPagedList<InvoiceSummary>> SearchInvoicesAsync(InvoiceQuery query, CancellationToken cancellationToken = default);

    /// <summary>R15: the paged, searchable, filterable order list, newest first.</summary>
    Task<IPagedList<SchoolOrderSummary>> SearchOrdersAsync(SchoolOrderQuery query, CancellationToken cancellationToken = default);
}
