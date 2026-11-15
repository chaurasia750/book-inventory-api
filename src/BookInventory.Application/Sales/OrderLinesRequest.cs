using BookInventory.Domain.Sales;

namespace BookInventory.Application.Sales;

/// <summary>
/// R3, R21: what the composer needs to turn request lines into order lines: the school, the date the prices are taken on and the
/// lines; for a change of an existing order also the item discount each of its lines has now, by line id (R36: an unchanged one is accepted).
/// </summary>
public sealed record OrderLinesRequest(
    Guid SchoolId, DateOnly OrderDate, IReadOnlyList<SchoolOrderLineRequest> Lines, IReadOnlyDictionary<Guid, Discount>? CurrentDiscounts = null);
