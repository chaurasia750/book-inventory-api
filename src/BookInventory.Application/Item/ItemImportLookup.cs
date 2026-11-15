using BookInventory.Application.Vendor;
using ItemRoot = BookInventory.Domain.Item.Item;

namespace BookInventory.Application.Item;

/// <summary>R19, R20: what the file is checked against: the company's items by code and by barcode (the matched ones loaded for change) and its vendors by name.</summary>
public sealed record ItemImportLookup(
    IReadOnlyDictionary<string, Guid> ByCode,
    IReadOnlyDictionary<string, Guid> ByBarcode,
    IReadOnlyDictionary<Guid, ItemRoot> Items,
    IReadOnlyDictionary<string, VendorReference> Vendors);
