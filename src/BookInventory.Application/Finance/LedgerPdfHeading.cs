namespace BookInventory.Application.Finance;

/// <summary>What the ledger PDF says about its own run: the filter in words, the From and To dates (R27) and when it was made.</summary>
public sealed record LedgerPdfHeading(string Filter, LedgerPeriod Period, string MadeAt);
