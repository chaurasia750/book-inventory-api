namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R53, R55: what a credit note needs of the invoice it names: its number, date, subtotal and total discount.</summary>
public sealed record ReturnedInvoice(Guid Id, string Number, DateOnly Date, decimal Subtotal, Discount TotalDiscount);
