namespace BookInventory.Domain.Sales;

/// <summary>Spec sales R64 (Q41): how a refund is paid: Cash, UPI or Bank transfer (the payment modes without the cheque).</summary>
public enum RefundMode
{
    Cash,
    Upi,
    BankTransfer
}
