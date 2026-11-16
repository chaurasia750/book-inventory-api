using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R57 (FR-RET-08, BR-LED-01): the ledger transaction of a credit note: credit the school's receivable the whole grand
/// total, good or damaged alike; debit sales return the taxable value and GST output the GST; the round-off the opposite way of an
/// invoice's. Zero lines are left out.
/// </summary>
public static class CreditNoteLedgerEntry
{
    public static LedgerPosting For(CreditNote note)
    {
        var t = note.Totals;
        var lines = new List<PostingLine>
        {
            PostingLine.Credit(LedgerAccount.SchoolReceivable, t.GrandTotal, new LedgerParty(PartyType.School, note.SchoolId))
        };
        if (t.Taxable > 0m)
            lines.Add(PostingLine.Debit(LedgerAccount.SalesReturn, t.Taxable));
        if (t.Gst > 0m)
            lines.Add(PostingLine.Debit(LedgerAccount.GstOutput, t.Gst));
        if (t.RoundOff > 0m)
            lines.Add(PostingLine.Debit(LedgerAccount.RoundOff, t.RoundOff));
        if (t.RoundOff < 0m)
            lines.Add(PostingLine.Credit(LedgerAccount.RoundOff, -t.RoundOff));
        return new LedgerPosting
        {
            Document = new LedgerDocument(CreditNote.DocumentType, note.Id, note.Number),
            TransactionDate = note.NoteDate,
            Session = note.Session,
            Narration = note.InvoiceNumber ?? note.Number,
            Lines = lines
        };
    }
}
