namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R54: one returned line: the item and price as the invoice line (or the school's last rate) had them, the quantity, the
/// condition and the item discount; the invoice line and order line it comes from when the credit note names an invoice.
/// </summary>
public sealed record CreditNoteLineInput(
    SaleItem Item, SalePrice Price, int Quantity, ReturnCondition Condition, Discount ItemDiscount, ReturnedFrom? From = null)
{
    /// <summary>
    /// R54 (FR-RET-02): a line returned from an invoice line, at its rate and item discount: a percentage as it is; a rupee discount
    /// of the whole invoice line shared by quantity (the whole quantity takes all of it).
    /// </summary>
    public static CreditNoteLineInput OfInvoiceLine(InvoiceLine line, int quantity, ReturnCondition condition)
    {
        var discount = line.ItemDiscount.Kind == DiscountKind.Amount && quantity != line.Quantity
            ? new Discount(DiscountKind.Amount, SaleMoney.Round2(line.ItemDiscountValue * quantity / line.Quantity))
            : line.ItemDiscount;
        var delivery = line.Delivery;
        return new CreditNoteLineInput(delivery.Item, delivery.Price, quantity, condition, discount, new ReturnedFrom(line.Id, line.OrderLineId, line.ClassName));
    }
}

/// <summary>Spec sales R54, R62: the invoice line (and its order line and class) a returned line comes from.</summary>
public sealed record ReturnedFrom(Guid InvoiceLineId, Guid OrderLineId, string? ClassName);
