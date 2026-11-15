using BookInventory.BuildingBlocks.Domain;
using BookInventory.Domain.Core;

namespace BookInventory.Domain.Finance;

/// <summary>
/// Spec finance R3, R14: one line of a <see cref="LedgerTransaction"/>: an account, the party on a party account, and a debit or a
/// credit. Created only by its transaction and never edited or deleted. It carries its own company (platform P18) and copies the
/// transaction's date, financial year and session so the statement filters the lines alone (R9). The audit trail skips it.
/// </summary>
[AuditIgnore]
public class LedgerLine : TransactionDomainBase, ICompanyOwned
{
    private LedgerLine()
    {
        FinancialYear = string.Empty;
    }

    public Guid CompanyId { get; private set; }

    public Guid LedgerTransactionId { get; private set; }

    public LedgerAccount Account { get; private set; }

    public PartyType? PartyType { get; private set; }

    public Guid? PartyId { get; private set; }

    public decimal DebitAmount { get; private set; }

    public decimal CreditAmount { get; private set; }

    public DateOnly TransactionDate { get; private set; }

    public string FinancialYear { get; private set; }

    public string? Session { get; private set; }

    public LedgerParty? Party => PartyType is { } type && PartyId is { } id ? new LedgerParty(type, id) : null;

    Guid? IAuditCompanyScope.AuditCompanyId => CompanyId == Guid.Empty ? null : CompanyId;

    public void AssignCompany(Guid companyId) => CompanyId = CompanyAssignment.Apply(CompanyId, companyId);

    /// <summary>R5: the opposite of this line, for a reversal.</summary>
    public PostingLine Opposite() => new(Account, Party, CreditAmount, DebitAmount);

    internal static LedgerLine Of(LedgerTransaction transaction, PostingLine line) =>
        new()
        {
            LedgerTransactionId = transaction.Id,
            Account = line.Account,
            PartyType = line.Party?.Type,
            PartyId = line.Party?.Id,
            DebitAmount = line.DebitAmount,
            CreditAmount = line.CreditAmount,
            TransactionDate = transaction.TransactionDate,
            FinancialYear = transaction.FinancialYear,
            Session = transaction.Session
        };
}
