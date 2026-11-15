using BookInventory.Domain.Finance;

namespace BookInventory.Application.Finance;

/// <summary>The human text of every <see cref="FinanceMessages"/> code; the code travels, the text is looked up (R55 of iam).</summary>
public static class FinanceMessageCatalog
{
    public static IReadOnlyDictionary<string, string> Messages { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [FinanceMessages.Unbalanced] = "The debits and credits of a ledger transaction must be equal.",
            [FinanceMessages.TooFewLines] = "A ledger transaction needs at least two lines.",
            [FinanceMessages.InvalidAmount] = "Each ledger line has either a debit or a credit greater than zero, with at most two decimals.",
            [FinanceMessages.InvalidParty] = "A party line must name its school, vendor or customer, and only a party account has a party.",
            [FinanceMessages.InvalidDocument] = "The document type, id and number are required.",
            [FinanceMessages.InvalidDate] = "The transaction date is required.",
            [FinanceMessages.InvalidText] = "The narration (500) or the session (20) is too long.",
            [FinanceMessages.AlreadyPosted] = "This document is already posted in the ledger.",
            [FinanceMessages.NotPosted] = "This document has no ledger posting to reverse.",
            [FinanceMessages.AlreadyReversed] = "This document's ledger posting is already reversed.",
            [FinanceMessages.PartyNotFound] = "The school or vendor could not be found.",
            [FinanceMessages.InvalidFilter] = "Give either a financial year (like 2026-27), a session, or a date range with from before to.",
            [FinanceMessages.TooManyLines] = "The statement has more than 5000 lines; narrow the filter.",
            [FinanceMessages.TooManyIds] = "Ask for the balances of at most 100 schools or vendors at a time."
        };
}
