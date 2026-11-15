using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R10 (BR-LED-01): the ledger transactions of a cheque result. A cleared cheque moves the money from cheque in hand to
/// the bank; a bounced one gives the school its debt back (debit the school's receivable, credit cheque in hand) with the reason as
/// narration. The document is the cheque (its mode id), so each result is posted once (finance R5).
/// </summary>
public static class ChequeLedgerEntry
{
    public const string ClearedDocumentType = "CHEQUE_CLEARED";
    public const string BouncedDocumentType = "CHEQUE_BOUNCED";

    public static LedgerPosting Cleared(Receipt receipt, ReceiptMode cheque) =>
        new()
        {
            Document = new LedgerDocument(ClearedDocumentType, cheque.Id, receipt.Number),
            TransactionDate = cheque.ResultDate!.Value,
            Narration = $"{receipt.Number}: cheque {cheque.ChequeNumber} cleared",
            Lines =
            [
                PostingLine.Debit(LedgerAccount.Bank, cheque.Amount),
                PostingLine.Credit(LedgerAccount.ChequeInHand, cheque.Amount)
            ]
        };

    public static LedgerPosting Bounced(Receipt receipt, ReceiptMode cheque) =>
        new()
        {
            Document = new LedgerDocument(BouncedDocumentType, cheque.Id, receipt.Number),
            TransactionDate = cheque.ResultDate!.Value,
            Narration = $"{receipt.Number}: cheque {cheque.ChequeNumber} bounced: {cheque.BounceReason}",
            Lines =
            [
                PostingLine.Debit(LedgerAccount.SchoolReceivable, cheque.Amount, new LedgerParty(PartyType.School, receipt.SchoolId)),
                PostingLine.Credit(LedgerAccount.ChequeInHand, cheque.Amount)
            ]
        };
}
