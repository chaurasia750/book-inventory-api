using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInventory.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Spec finance R6 (plan finance#02): the one-time catch-up of the opening balances that schools and vendors already had
    /// before the ledger existed. Data only and insert-only: for every live school and vendor with an opening balance above zero
    /// and no opening transaction yet, one <c>OPENING_BALANCE</c> transaction with its two lines, dated the business date
    /// (Asia/Kolkata), no session, posted by the system account (empty id). Running it again posts nothing. The accounts are the
    /// ones of <c>OpeningBalanceEntry</c>: a school debit SCHOOL_RECEIVABLE / credit OPENING_BALANCE, a vendor the opposite side.
    /// Ids are UUID v7 made in SQL (PostgreSQL 17 has no uuidv7()).
    /// </summary>
    public partial class Finance_OpeningBalanceCatchUp : Migration
    {
        private const string UuidV7 =
            "encode(set_bit(set_bit(overlay(uuid_send(gen_random_uuid()) placing substring(int8send(floor(extract(epoch from clock_timestamp()) * 1000)::bigint) from 3) from 1 for 6), 52, 1), 53, 1), 'hex')::uuid";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(CatchUp("school.school", "SCHOOL", "SCHOOL_RECEIVABLE", partyOnDebit: true));
            migrationBuilder.Sql(CatchUp("vendor.vendor", "VENDOR", "VENDOR_PAYABLE", partyOnDebit: false));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Insert-only data: removing posted ledger entries is a data change the owner approves on its own, never a rollback step.
        }

        private static string CatchUp(string partyTable, string partyType, string partyAccount, bool partyOnDebit) => $"""
            CREATE TEMP TABLE ob_catch_up ON COMMIT DROP AS
            SELECT p.id AS party_id, p.code, p.company_id, p.opening_balance_amount AS amount,
                   {UuidV7} AS transaction_id, {UuidV7} AS document_id,
                   {UuidV7} AS party_line_id, {UuidV7} AS opening_line_id,
                   (now() AT TIME ZONE 'Asia/Kolkata')::date AS day
            FROM {partyTable} p
            WHERE NOT p.is_deleted AND p.opening_balance_amount > 0
              AND NOT EXISTS (
                SELECT 1 FROM finance.ledger_line l
                JOIN finance.ledger_transaction t ON t.id = l.ledger_transaction_id
                WHERE t.document_type = 'OPENING_BALANCE' AND l.company_id = p.company_id
                  AND l.party_type = '{partyType}' AND l.party_id = p.id);

            INSERT INTO finance.ledger_transaction (id, transaction_date, financial_year, session, document_type, document_id,
                document_number, posting_kind, narration, is_deleted, created_at, created_by, company_id)
            SELECT transaction_id, day, {FinancialYear("day")}, NULL, 'OPENING_BALANCE', document_id,
                code, 'POST', NULL, false, now(), '00000000-0000-0000-0000-000000000000'::uuid, company_id
            FROM ob_catch_up;

            INSERT INTO finance.ledger_line (id, company_id, ledger_transaction_id, account, party_type, party_id,
                debit_amount, credit_amount, transaction_date, financial_year, session, is_deleted, created_at, created_by)
            SELECT party_line_id, company_id, transaction_id, '{partyAccount}', '{partyType}', party_id,
                {(partyOnDebit ? "amount, 0" : "0, amount")}, day, {FinancialYear("day")}, NULL, false, now(), '00000000-0000-0000-0000-000000000000'::uuid
            FROM ob_catch_up
            UNION ALL
            SELECT opening_line_id, company_id, transaction_id, 'OPENING_BALANCE', NULL, NULL,
                {(partyOnDebit ? "0, amount" : "amount, 0")}, day, {FinancialYear("day")}, NULL, false, now(), '00000000-0000-0000-0000-000000000000'::uuid
            FROM ob_catch_up;

            DROP TABLE ob_catch_up;
            """;

        /// <summary>The April-March label of R4 (<c>2026-27</c>), the same as <c>FinancialYear.Of</c>.</summary>
        private static string FinancialYear(string day) =>
            $"(CASE WHEN extract(month FROM {day}) >= 4 THEN extract(year FROM {day})::int ELSE extract(year FROM {day})::int - 1 END)::text || '-' || "
            + $"lpad(((CASE WHEN extract(month FROM {day}) >= 4 THEN extract(year FROM {day})::int ELSE extract(year FROM {day})::int - 1 END + 1) % 100)::text, 2, '0')";
    }
}
