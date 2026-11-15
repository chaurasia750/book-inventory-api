namespace BookInventory.Application.Finance;

/// <summary>R18: a rendered ledger statement: the PDF bytes and the file name (<c>&lt;party code&gt;-ledger.pdf</c>).</summary>
public sealed record LedgerPdf(byte[] Content, string FileName);
