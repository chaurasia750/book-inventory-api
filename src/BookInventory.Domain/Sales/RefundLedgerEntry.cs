using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R66 (FR-LED-01 "Refund (Dr)"): the ledger transaction of a refund: debit the school's receivable the amount (its
/// balance rises); credit Cash for a cash refund and Bank for a UPI or bank-transfer one (the payment receipt's accounts the other way round).
/// </summary>
public static class RefundLedgerEntry
{
    public static LedgerPosting For(SchoolRefund refund) =>
        new()
        {
            Document = new LedgerDocument(SchoolRefund.DocumentType, refund.Id, refund.Number),
            TransactionDate = refund.RefundDate,
            Narration = refund.CreditNoteNumber ?? refund.Number,
            Lines =
            [
                PostingLine.Debit(LedgerAccount.SchoolReceivable, refund.Amount, new LedgerParty(PartyType.School, refund.SchoolId)),
                PostingLine.Credit(refund.Mode == RefundMode.Cash ? LedgerAccount.Cash : LedgerAccount.Bank, refund.Amount)
            ]
        };
}
