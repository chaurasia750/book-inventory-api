using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Payment;

namespace BookInventory.Application.Payment;

/// <summary>
/// The persistence port of the <c>payment</c> module. Every read is already limited to the session's company by the platform
/// filter (P19), so nothing here takes a company. Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface IReceiptStore
{
    Task AddAsync(Receipt receipt, CancellationToken cancellationToken = default);

    /// <summary>R9: the id of the receipt already recorded for this client request, or <c>null</c>.</summary>
    Task<Guid?> FindIdByClientRequestAsync(Guid clientRequestId, CancellationToken cancellationToken = default);

    /// <summary>The receipt about to change: tracked, with its modes and allocations; <c>null</c> for an unknown id or another company's.</summary>
    Task<Receipt?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R12: the receipts that have something applied to this invoice: tracked, with their modes and allocations.</summary>
    Task<IReadOnlyList<Receipt>> FindAppliedToInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>The receipt for reading: not tracked, with its modes and allocations; <c>null</c> for an unknown id or another company's.</summary>
    Task<Receipt?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R16: the paged, filterable list, newest first.</summary>
    Task<IPagedList<ReceiptRow>> SearchAsync(ReceiptQuery query, CancellationToken cancellationToken = default);

    /// <summary>R3, R6: for each of these invoices the sum of its allocation rows over all receipts; an invoice with none is left out.</summary>
    Task<IReadOnlyDictionary<Guid, decimal>> AllocatedAsync(IReadOnlyCollection<Guid> invoiceIds, CancellationToken cancellationToken = default);
}
