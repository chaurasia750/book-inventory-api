using BookInventory.Domain.Item;

namespace BookInventory.Application.Item;

/// <summary>
/// R16/R17: what another module may know about an item: its id, code, name, whether it can be chosen for a new document and
/// whether its quantity is tracked in stock.
/// </summary>
public sealed record ItemReference(Guid Id, string Code, string Name, bool IsActive, bool IsStockManaged);

/// <summary>R16 (for the sales module): what a sales line keeps of an item: its reference, HSN code, GST rate and category (for the books count of the counter day summary, sales R50).</summary>
public sealed record ItemSaleReference(ItemReference Item, string? HsnCode, decimal GstRatePercent, ItemCategory Category = ItemCategory.Other);

/// <summary>
/// R16/R17: which items a caller means: any mix of vendor, category and class (ignoring case), optionally only active ones and
/// optionally only stock-managed (<c>true</c>) or not stock-managed (<c>false</c>) ones; empty means all.
/// </summary>
public sealed record ItemSelection(
    Guid? VendorId = null,
    ItemCategory? Category = null,
    string? ClassName = null,
    bool OnlyActive = false,
    bool? StockManaged = null);

/// <summary>R16: one page of item ids for a search text and a selection, in the given order (<c>Name</c> or <c>Code</c>, then <c>ASC</c> or <c>DESC</c>).</summary>
public sealed record ItemPageRequest(string? Search, ItemSelection Selection, string? OrderBy, int PageIndex, int PageSize);

/// <summary>One page of item ids and the total number of matches.</summary>
public sealed record ItemIdPage(IReadOnlyList<Guid> Ids, int TotalCount);

/// <summary>
/// R16: the capability other modules (the price module first) use to read items, never by joining the item tables. It answers
/// only for the company of the session (platform P19), returns plain values and never the aggregate, and changes nothing.
/// </summary>
public interface IItemDirectory
{
    /// <summary>The cap of <see cref="AllIdsAsync"/>: a bulk request is bounded, a company with more items narrows the selection.</summary>
    const int MaxBulkItems = 5000;

    /// <summary>The item's id, code, name and active flag, or <c>null</c> for an unknown id or another company's.</summary>
    Task<ItemReference?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>The code and name of the given items, by id; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, ItemReference>> ReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>One page of item ids that match the search text (name, code, barcode, class, subject, vendor name) and the selection.</summary>
    Task<ItemIdPage> PageAsync(ItemPageRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// R16 (for stock R18): the items whose code or barcode equals one of these texts (ignoring case and surrounding spaces), keyed
    /// by the upper-case text; a code wins over a barcode of another item. A text that matches nothing is left out.
    /// </summary>
    Task<IReadOnlyDictionary<string, ItemReference>> FindByCodeOrBarcodeAsync(IReadOnlyCollection<string> keys, CancellationToken cancellationToken = default);

    /// <summary>R16 (for the sales module): the sale values of the given items; an id that is not found is left out.</summary>
    Task<IReadOnlyDictionary<Guid, ItemSaleReference>> SaleReferencesAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Every item id of the selection, at most <see cref="MaxBulkItems"/>.</summary>
    Task<IReadOnlyList<Guid>> AllIdsAsync(ItemSelection selection, CancellationToken cancellationToken = default);
}
