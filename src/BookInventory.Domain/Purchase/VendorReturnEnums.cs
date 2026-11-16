namespace BookInventory.Domain.Purchase;

/// <summary>Spec purchase R8: a vendor return is active, or cancelled (it keeps its number).</summary>
public enum VendorReturnStatus
{
    Active,
    Cancelled
}

/// <summary>Spec purchase R4 (FR-RET-11, FR-STK-07): where a returned line comes from: Main stock (good) or Damaged stock.</summary>
public enum ReturnStockSource
{
    Main,
    Damaged
}
