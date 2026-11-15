namespace BookInventory.Domain.Sales;

/// <summary>R45: how a counter bill is paid at once: Cash or UPI. There is no credit at the school counter.</summary>
public enum CounterPaymentMode
{
    Cash,

    Upi
}
