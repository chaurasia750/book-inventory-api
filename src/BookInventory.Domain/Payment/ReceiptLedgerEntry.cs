using System.Globalization;
using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Payment;

/// <summary>
/// Spec payment R7 (BR-LED-01): the ledger transaction of a receipt: credit the school's receivable the total; debit cash for the
/// cash modes, bank for UPI and bank transfer, cheque in hand for cheques (finance R2). Zero lines are left out. The payment is
/// received on the school's ledger; the allocation to invoices is a matching that the ledger never needs.
/// </summary>
public static class ReceiptLedgerEntry
{
    public const string DocumentType = "PAYMENT_RECEIPT";

    public static LedgerPosting For(Receipt receipt)
    {
        var lines = new List<PostingLine>
        {
            PostingLine.Credit(LedgerAccount.SchoolReceivable, receipt.TotalAmount, new LedgerParty(PartyType.School, receipt.SchoolId))
        };
        AddDebit(lines, LedgerAccount.Cash, receipt.Modes.Where(m => m.ModeType == ModeType.Cash));
        AddDebit(lines, LedgerAccount.Bank, receipt.Modes.Where(m => m.ModeType is ModeType.Upi or ModeType.BankTransfer));
        AddDebit(lines, LedgerAccount.ChequeInHand, receipt.Modes.Where(m => m.ModeType == ModeType.Cheque));

        return new LedgerPosting
        {
            Document = new LedgerDocument(DocumentType, receipt.Id, receipt.Number),
            TransactionDate = receipt.ReceiptDate,
            Narration = Narration(receipt),
            Lines = lines
        };
    }

    private static void AddDebit(List<PostingLine> lines, LedgerAccount account, IEnumerable<ReceiptMode> modes)
    {
        var amount = modes.Sum(m => m.Amount);
        if (amount > 0m)
            lines.Add(PostingLine.Debit(account, amount));
    }

    private static string Narration(Receipt receipt)
    {
        var parts = receipt.Modes.Select(m => $"{Label(m.ModeType)} {m.Amount.ToString("0.00", CultureInfo.InvariantCulture)}");
        return $"{receipt.Number}: {string.Join(", ", parts)}";
    }

    private static string Label(ModeType type) => type switch
    {
        ModeType.Cash => "Cash",
        ModeType.Upi => "UPI",
        ModeType.BankTransfer => "Bank transfer",
        _ => "Cheque"
    };
}
