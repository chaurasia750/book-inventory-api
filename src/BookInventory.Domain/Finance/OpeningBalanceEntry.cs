namespace BookInventory.Domain.Finance;

/// <summary>
/// Spec finance R6: the ledger transaction of a party's opening balance change. A school's (or customer's) opening is what it owes:
/// debit its receivable, credit <see cref="LedgerAccount.OpeningBalance"/>; a vendor's is what the company owes: debit
/// <see cref="LedgerAccount.OpeningBalance"/>, credit its payable. A lowered balance posts the opposite. No session (Q10).
/// </summary>
public static class OpeningBalanceEntry
{
    public const string DocumentType = "OPENING_BALANCE";

    /// <summary>The posting for a change from <paramref name="change"/>'s old to its new amount, or null when nothing changed.</summary>
    public static LedgerPosting? For(LedgerParty party, string partyCode, OpeningBalanceChange change, DateOnly date)
    {
        var difference = change.NewAmount - change.OldAmount;
        if (difference == 0)
            return null;

        var account = PartyAccountOf(party.Type);
        var amount = Math.Abs(difference);
        var partyOnDebit = (party.Type != PartyType.Vendor) == (difference > 0);
        IReadOnlyList<PostingLine> lines = partyOnDebit
            ? [PostingLine.Debit(account, amount, party), PostingLine.Credit(LedgerAccount.OpeningBalance, amount)]
            : [PostingLine.Debit(LedgerAccount.OpeningBalance, amount), PostingLine.Credit(account, amount, party)];

        return new LedgerPosting
        {
            Document = new LedgerDocument(DocumentType, Guid.CreateVersion7(), partyCode),
            TransactionDate = date,
            Lines = lines
        };
    }

    private static LedgerAccount PartyAccountOf(PartyType type) =>
        type switch
        {
            PartyType.School => LedgerAccount.SchoolReceivable,
            PartyType.Vendor => LedgerAccount.VendorPayable,
            _ => LedgerAccount.CustomerReceivable
        };
}
