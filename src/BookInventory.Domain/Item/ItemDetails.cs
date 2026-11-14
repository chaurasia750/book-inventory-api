namespace BookInventory.Domain.Item;

/// <summary>
/// R2-R7: the master data of an item as it arrives; <see cref="Item"/> validates and normalises every value. A missing GST
/// rate is 0 and a missing stock-managed flag is the default of the category (R4, R7).
/// </summary>
public sealed record ItemDetails(
    string Name,
    ItemCategory Category,
    Guid? VendorId = null,
    string? ClassName = null,
    string? Subject = null,
    string? HsnCode = null,
    decimal? GstRatePercent = null,
    bool? IsStockManaged = null);
