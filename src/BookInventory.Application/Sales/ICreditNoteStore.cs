using BookInventory.BuildingBlocks.Domain.Models;
using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>What a school was last invoiced for an item: the price snapshot of its latest live invoice line (R54, Q35).</summary>
public sealed record LastInvoicedPrice(Guid ItemId, Guid PriceVersionId, string PriceVersionName, decimal MrpAmount, decimal RateAmount);

/// <summary>
/// The persistence port of the credit notes (spec sales R53-R63). Every read is already limited to the session's company by the
/// platform filter (P19). Like the other stores it has no save method: the request transaction commits.
/// </summary>
public interface ICreditNoteStore
{
    Task AddAsync(CreditNote note, CancellationToken cancellationToken = default);

    /// <summary>The credit note about to be cancelled: tracked, with its lines. <c>null</c> for an unknown id or another company's.</summary>
    Task<CreditNote?> FindForManagementAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CreditNote?> FindForReadAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>R61: the paged, searchable, filterable list, newest first by default.</summary>
    Task<IPagedList<CreditNote>> SearchAsync(CreditNoteSearchQuery query, CancellationToken cancellationToken = default);

    /// <summary>R54: the quantity live credit notes returned, by invoice line, for one invoice.</summary>
    Task<IReadOnlyDictionary<Guid, int>> ReturnedQuantitiesAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>R54, Q35: the latest live invoice line price of each item for a school.</summary>
    Task<IReadOnlyDictionary<Guid, LastInvoicedPrice>> LastInvoicedPricesAsync(Guid schoolId, IReadOnlyCollection<Guid> itemIds, CancellationToken cancellationToken = default);

    /// <summary>R61: the credit notes of an invoice, newest first.</summary>
    Task<IReadOnlyList<CreditNote>> OfInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);

    /// <summary>R70: whether an invoice has a live credit note (it then cannot be cancelled).</summary>
    Task<bool> HasLiveForInvoiceAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}
