namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R12: an invoice is issued, or cancelled (it keeps its number).</summary>
public enum InvoiceStatus
{
    Issued,
    Cancelled
}
