namespace BookInventory.Domain.Payment;

/// <summary>Spec payment R10: the status of a cheque: pending when received, then cleared or bounced, once.</summary>
public enum ChequeStatus
{
    Pending,
    Cleared,
    Bounced
}
