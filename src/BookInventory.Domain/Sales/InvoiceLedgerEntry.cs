using BookInventory.Domain.Finance;

namespace BookInventory.Domain.Sales;

/// <summary>
/// Spec sales R11 (BR-LED-01): the ledger transaction of an invoice: debit the school's receivable the grand total; credit sales the
/// taxable value and GST output the GST; the round-off credited when positive, debited when negative. Zero lines are left out.
/// </summary>
public static class InvoiceLedgerEntry
{
    public const string DocumentType = "SCHOOL_INVOICE";

    public static LedgerPosting For(Invoice invoice)
    {
        var t = invoice.Totals;
        var lines = new List<PostingLine>
        {
            PostingLine.Debit(LedgerAccount.SchoolReceivable, t.GrandTotal, new LedgerParty(PartyType.School, invoice.SchoolId))
        };
        if (t.Taxable > 0m)
            lines.Add(PostingLine.Credit(LedgerAccount.Sales, t.Taxable));
        if (t.Gst > 0m)
            lines.Add(PostingLine.Credit(LedgerAccount.GstOutput, t.Gst));
        if (t.RoundOff > 0m)
            lines.Add(PostingLine.Credit(LedgerAccount.RoundOff, t.RoundOff));
        if (t.RoundOff < 0m)
            lines.Add(PostingLine.Debit(LedgerAccount.RoundOff, -t.RoundOff));

        return new LedgerPosting
        {
            Document = new LedgerDocument(DocumentType, invoice.Id, invoice.Number),
            TransactionDate = invoice.InvoiceDate,
            Session = invoice.Session,
            Narration = invoice.OrderNumber,
            Lines = lines
        };
    }
}
