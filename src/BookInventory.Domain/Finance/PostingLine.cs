namespace BookInventory.Domain.Finance;

/// <summary>Spec finance R3: one line to post: an account, the party when it is a party account, and a debit or a credit.</summary>
public sealed record PostingLine(LedgerAccount Account, LedgerParty? Party, decimal DebitAmount, decimal CreditAmount)
{
    public static PostingLine Debit(LedgerAccount account, decimal amount, LedgerParty? party = null) => new(account, party, amount, 0m);

    public static PostingLine Credit(LedgerAccount account, decimal amount, LedgerParty? party = null) => new(account, party, 0m, amount);
}
