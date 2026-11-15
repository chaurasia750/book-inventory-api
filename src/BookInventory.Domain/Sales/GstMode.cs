namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R7 (BR-GST-05, BR-GST-06): no GST, CGST + SGST (same state) or IGST (another state).</summary>
public enum GstMode
{
    NotApplied,
    SameState,
    OtherState
}
