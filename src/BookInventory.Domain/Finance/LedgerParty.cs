namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R3: a school, vendor or customer named on a party-account line.</summary>
public sealed record LedgerParty(PartyType Type, Guid Id)
{
    /// <summary>R3: the party account of each party type.</summary>
    public static PartyType? PartyTypeOf(LedgerAccount account) =>
        account switch
        {
            LedgerAccount.SchoolReceivable => PartyType.School,
            LedgerAccount.CustomerReceivable => PartyType.Customer,
            LedgerAccount.VendorPayable => PartyType.Vendor,
            _ => null
        };

    /// <summary>R7: a school's or customer's balance is debit minus credit; a vendor's is credit minus debit.</summary>
    public static decimal BalanceOf(PartyType type, decimal debit, decimal credit) =>
        type == PartyType.Vendor ? credit - debit : debit - credit;
}
