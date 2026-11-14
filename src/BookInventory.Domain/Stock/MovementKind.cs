namespace BookInventory.Domain.Stock;

/// <summary>Spec stock R5: why a stock entry was posted. Which stock and direction each kind allows is <see cref="MovementKindRules"/>.</summary>
public enum MovementKind
{
    Opening,
    Adjustment,
    Purchase,
    SchoolInvoice,
    CounterSale,
    SchoolReturnGood,
    SchoolReturnDamaged,
    VendorReturn,
    SampleFromVendor,
    SampleToSchool,
    SampleFromSchool,
    SampleToVendor,
    Reversal
}
