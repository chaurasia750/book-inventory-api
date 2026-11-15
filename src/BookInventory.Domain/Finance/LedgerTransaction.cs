using BookInventory.BuildingBlocks.Domain.Exceptions;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Finance;

/// <summary>
/// Spec finance R3-R5, R8, R12, D1: one balanced double-entry posting of one document. It is created complete by
/// <see cref="Post"/>, which checks every line rule and that the debits equal the credits, and never changes afterwards; a cancel
/// is a second transaction built by <see cref="Reverse"/> with the opposite lines (BR-LED-05). The transactions are the history
/// themselves, so the audit trail skips them (R14).
/// </summary>
[AuditIgnore]
public class LedgerTransaction : CompanyOwnedRoot
{
    /// <summary>R8: the largest amount <c>numeric(18,2)</c> holds.</summary>
    public const decimal MaxAmount = 9_999_999_999_999_999.99m;

    public const int NarrationMaxLength = 500;
    public const int SessionMaxLength = 20;

    private readonly List<LedgerLine> _lines = [];

    private LedgerTransaction()
    {
        FinancialYear = string.Empty;
        DocumentType = string.Empty;
        DocumentNumber = string.Empty;
    }

    public DateOnly TransactionDate { get; private set; }

    public string FinancialYear { get; private set; }

    public string? Session { get; private set; }

    public string DocumentType { get; private set; }

    public Guid DocumentId { get; private set; }

    public string DocumentNumber { get; private set; }

    public PostingKind PostingKind { get; private set; }

    public string? Narration { get; private set; }

    public IReadOnlyList<LedgerLine> Lines => _lines;

    public LedgerDocument Document => new(DocumentType, DocumentId, DocumentNumber);

    /// <summary>R3, R4, R8: a document's transaction; refused unless every line is valid and the debits equal the credits.</summary>
    public static LedgerTransaction Post(LedgerPosting posting) => Build(posting, PostingKind.Post);

    /// <summary>R5: the cancel of this transaction: the same accounts and parties with every debit and credit swapped, dated <paramref name="reversalDate"/>.</summary>
    public LedgerTransaction Reverse(DateOnly reversalDate)
    {
        if (PostingKind == PostingKind.Reversal)
            throw new ConflictException(FinanceMessages.AlreadyReversed);

        return Build(
            new LedgerPosting
            {
                Document = Document,
                TransactionDate = reversalDate,
                Session = Session,
                Narration = Narration,
                Lines = [.. _lines.Select(l => l.Opposite())]
            },
            PostingKind.Reversal);
    }

    private static LedgerTransaction Build(LedgerPosting posting, PostingKind kind)
    {
        RequireHeader(posting);
        RequireLines(posting.Lines);

        var transaction = new LedgerTransaction
        {
            TransactionDate = posting.TransactionDate,
            FinancialYear = Finance.FinancialYear.Of(posting.TransactionDate),
            Session = Trimmed(posting.Session),
            DocumentType = posting.Document.DocumentType.Trim(),
            DocumentId = posting.Document.DocumentId,
            DocumentNumber = posting.Document.DocumentNumber.Trim(),
            PostingKind = kind,
            Narration = Trimmed(posting.Narration)
        };
        transaction._lines.AddRange(posting.Lines.Select(l => LedgerLine.Of(transaction, l)));
        transaction.AddDomainEvent(new LedgerPosted(
            transaction.DocumentType, transaction.DocumentId, kind, [.. posting.Lines.Where(l => l.Party is not null).Select(l => l.Party!).Distinct()]));
        return transaction;
    }

    private static void RequireHeader(LedgerPosting posting)
    {
        var document = posting.Document;
        if (document.DocumentId == Guid.Empty
            || string.IsNullOrWhiteSpace(document.DocumentType) || document.DocumentType.Trim().Length > LedgerDocument.TypeMaxLength
            || string.IsNullOrWhiteSpace(document.DocumentNumber) || document.DocumentNumber.Trim().Length > LedgerDocument.NumberMaxLength)
            throw new ValidationFailureException(FinanceMessages.InvalidDocument);
        if (posting.TransactionDate == default)
            throw new ValidationFailureException(FinanceMessages.InvalidDate);
        if (Trimmed(posting.Narration)?.Length > NarrationMaxLength || Trimmed(posting.Session)?.Length > SessionMaxLength)
            throw new ValidationFailureException(FinanceMessages.InvalidText);
    }

    /// <summary>R3, R8: two lines or more, each one side with a valid amount and the right party, and the sides equal.</summary>
    private static void RequireLines(IReadOnlyList<PostingLine> lines)
    {
        if (lines.Count < 2)
            throw new ValidationFailureException(FinanceMessages.TooFewLines);

        foreach (var line in lines)
        {
            if ((line.DebitAmount > 0) == (line.CreditAmount > 0) || line.DebitAmount < 0 || line.CreditAmount < 0
                || !IsAmount(line.DebitAmount + line.CreditAmount))
                throw new ValidationFailureException(FinanceMessages.InvalidAmount);
            if (LedgerParty.PartyTypeOf(line.Account) != line.Party?.Type || line.Party?.Id == Guid.Empty)
                throw new ValidationFailureException(FinanceMessages.InvalidParty);
        }

        if (lines.Sum(l => l.DebitAmount) != lines.Sum(l => l.CreditAmount))
            throw new ValidationFailureException(FinanceMessages.Unbalanced);
    }

    /// <summary>R8: greater than zero, at most two decimals, within <c>numeric(18,2)</c>; never rounded.</summary>
    private static bool IsAmount(decimal amount) => amount is > 0 and <= MaxAmount && decimal.Round(amount, 2) == amount;

    private static string? Trimmed(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
