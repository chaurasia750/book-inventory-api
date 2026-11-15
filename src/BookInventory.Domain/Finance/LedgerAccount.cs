namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R2: the fixed system accounts, the same for every company. The first three are the party accounts.</summary>
public enum LedgerAccount
{
    SchoolReceivable,
    CustomerReceivable,
    VendorPayable,
    Sales,
    SalesReturn,
    Purchase,
    PurchaseReturn,
    GstOutput,
    GstInput,
    Cash,
    Bank,
    ChequeInHand,
    DiscountAllowed,
    RoundOff,
    OpeningBalance
}
