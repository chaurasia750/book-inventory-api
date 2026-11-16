using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Purchase;

/// <summary>
/// Spec purchase R7 (BR-LED-02, FR-LED-02 "Debit Note (Dr)"): the ledger transaction of a vendor return: debit the vendor's payable
/// the total (the vendor's balance falls); credit purchase return the sum of the line amounts; the round-off on round-off (credit when
/// the total is above the sum, debit when below).
/// </summary>
public static class VendorReturnLedgerEntry
{
    public static LedgerPosting For(VendorReturn vendorReturn)
    {
        var lines = new List<PostingLine>
        {
            PostingLine.Debit(LedgerAccount.VendorPayable, vendorReturn.GrandTotalAmount, new LedgerParty(PartyType.Vendor, vendorReturn.VendorId)),
            PostingLine.Credit(LedgerAccount.PurchaseReturn, vendorReturn.SubtotalAmount)
        };
        if (vendorReturn.RoundOffAmount > 0m)
            lines.Add(PostingLine.Credit(LedgerAccount.RoundOff, vendorReturn.RoundOffAmount));
        if (vendorReturn.RoundOffAmount < 0m)
            lines.Add(PostingLine.Debit(LedgerAccount.RoundOff, -vendorReturn.RoundOffAmount));
        return new LedgerPosting
        {
            Document = new LedgerDocument(VendorReturn.DocumentType, vendorReturn.Id, vendorReturn.Number),
            TransactionDate = vendorReturn.ReturnDate,
            Narration = vendorReturn.Number,
            Lines = lines
        };
    }
}
