namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R26: how an invoice is shown and printed: lines of the same book merged (a plain book list) or grouped under each class.</summary>
public enum InvoiceLayout
{
    BookList,
    ByClass
}
