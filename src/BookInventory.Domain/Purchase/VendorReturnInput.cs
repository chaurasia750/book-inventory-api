using BookInventory.Domain.Sales;

namespace BookInventory.Domain.Purchase;

/// <summary>Spec purchase R4: one returned line: the item as it is now named, where it leaves from, the rate and the quantity.</summary>
public sealed record VendorReturnLineInput(Guid ItemId, string ItemCode, string ItemName, ReturnStockSource Source, decimal Rate, int Quantity);

/// <summary>Spec purchase R10: what a vendor return keeps of the vendor.</summary>
public sealed record ReturnVendor(Guid Id, string Code, string Name);

/// <summary>Spec purchase R2-R5, R10: everything a vendor return is made from: its number and date, the vendor, the company's identity and the lines.</summary>
public sealed record VendorReturnInput
{
    public required string Number { get; init; }

    public required DateOnly ReturnDate { get; init; }

    public required DateOnly Today { get; init; }

    public required ReturnVendor Vendor { get; init; }

    public required CompanyInvoiceSnapshot CompanySnapshot { get; init; }

    public string? Remark { get; init; }

    public required IReadOnlyList<VendorReturnLineInput> Lines { get; init; }
}
