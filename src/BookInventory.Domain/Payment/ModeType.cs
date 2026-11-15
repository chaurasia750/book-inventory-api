namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R5: how money came (BRD 4.14 A): cash, UPI, bank transfer or cheque.</summary>
public enum ModeType
{
    Cash,
    Upi,
    BankTransfer,
    Cheque
}
