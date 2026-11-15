namespace BookInventory.Domain.Finance;

/// <summary>
/// Every message code of the <c>finance</c> module (spec R55 of iam applies to all modules). The domain throws these codes; the
/// human text lives in <c>BookInventory.Application.Finance.FinanceMessageCatalog</c>. Codes are stable API surface.
/// </summary>
public static class FinanceMessages
{
    public const string Unbalanced = "finance.unbalanced";
    public const string TooFewLines = "finance.too_few_lines";
    public const string InvalidAmount = "finance.invalid_amount";
    public const string InvalidParty = "finance.invalid_party";
    public const string InvalidDocument = "finance.invalid_document";
    public const string InvalidDate = "finance.invalid_date";
    public const string InvalidText = "finance.invalid_text";
    public const string AlreadyPosted = "finance.already_posted";
    public const string NotPosted = "finance.not_posted";
    public const string AlreadyReversed = "finance.already_reversed";
    public const string PartyNotFound = "finance.party_not_found";
    public const string InvalidFilter = "finance.invalid_filter";
    public const string TooManyLines = "finance.too_many_lines";
    public const string TooManyIds = "finance.too_many_ids";
}
