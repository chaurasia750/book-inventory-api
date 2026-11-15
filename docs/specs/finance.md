# Spec: finance - Ledger

- **Module:** `finance` (PostgreSQL schema `finance`)
- **Status:** R17 and AC9 Approved by abhishek in chat on 2026-11-15 (the school and vendor profile questions: profile panel with tabs, balance column in the lists, the same for vendors); R1-R16 Approved by abhishek in chat on 2026-11-14 (Q1-Q11 answered one by one, all with the recommended answers; D1-D4 with them)
- **Last updated:** 2026-11-14
- **Source docs:** [BRD §2, §4.15 Ledger (FR-LED-01 to FR-LED-10), §5.1 BR-SES-01 to BR-SES-03 and BR-SES-07, §5.5 the stock movement table (ledger columns), §5.6 BR-LED-01 to BR-LED-05, §8 Data Model (PartyLedger) and design decisions 2 and 4](../bussiness/BRD-Book-Inventory-Phase1.md); [CLAUDE.project.md](../../CLAUDE.project.md) "Money and ledger"; [specs/platform.md](platform.md) P18-P21; [specs/school.md](school.md) R7, R16, R17, D3, D7; [specs/vendor.md](vendor.md) R6, R15, R16; [specs/stock.md](stock.md) (the posting capability it mirrors)
- **Plans:** [finance#05](../plans/finance/2026-11-15-05-party-balances.md) (Done, R17, AC9), [finance#06](../plans/finance/2026-11-15-06-ui-party-profile-ledger.md) (In progress, UI), [finance#01](../plans/finance/2026-11-14-01-ledger-core.md) (Done), [finance#02](../plans/finance/2026-11-14-02-opening-balances.md) (Done), [finance#03](../plans/finance/2026-11-14-03-ledger-statement.md) (Done), [finance#04](../plans/finance/2026-11-14-04-ui-ledger-screens.md) (Superseded by finance#06); what it needs from other modules: [school#03](../plans/school/2026-11-14-03-school-directory.md) (Done), [school#04](../plans/school/2026-11-14-04-opening-balance-event.md) (Done), [vendor#04](../plans/vendor/2026-11-14-04-opening-balance-event.md) (Done)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`finance` keeps the company's money record: every money movement is one balanced double-entry **ledger transaction** (debits equal credits), and the **party ledger** of a school or a vendor is the part of those transactions that touches that party. A balance is always the sum of the entries, never a number typed over (BRD §8 design decision 2); a posted entry is never changed or deleted, a mistake is undone by a reversing transaction (BR-LED-05).

Feature **Ledger foundation** (R1-R16): the ledger itself and the one capability through which later modules post (School Invoice, Payment, Return, Purchase, Sample bill); the opening balances of schools and vendors in the ledger; the school and vendor ledger statement with its filters and running balance; and the outstanding list of all schools and all vendors.

Out of scope for this feature: the documents that post most transactions (Invoice, Payment, Return, Purchase: their own modules, which call R5); the session of a date and its cut-offs (school D3/D7: they come with the first order feature, R4); the "Pending" mark of a cheque (FR-LED-05: the Payment module); opening a document from the statement (FR-LED-09: when documents exist); the PDF and Excel of a statement (FR-LED-08, Q7); the customer ledger (FR-LED-11: the parked customer module, its party type is foreseen in R3); reports beyond the outstanding list (§4.16).

## 2. Concepts and glossary

- **Account**: one of the company's fixed system accounts (R2), for example school receivable, vendor payable, sales, cash.
- **Party**: a school, a vendor or (later) a customer; a party account line names the party (R3).
- **Ledger transaction**: one balanced posting of one document: a date, the financial year, an optional session, the document it comes from and at least two lines (R3).
- **Party ledger**: the lines of one party, oldest first, with the running balance (R9).
- **Balance**: for a school (or customer), debits minus credits = what the party owes the company; for a vendor, credits minus debits = what the company owes the vendor (BR-LED-01, BR-LED-02).
- **Financial year**: April to March of the entry's date, written `2026-27` (BR-SES-01, BR-SES-02).
- **Session**: the school's session of the document (`2027-28` or `2027`), given by the posting document (BR-SES-03, R4).

## 3. Business rules

**Ledger foundation**
- **R1** The ledger belongs to exactly one company, the company of the session. Another company's entries are never read or written (platform P18-P21). Schools and vendors are read through their directory capabilities ([specs/school.md](school.md) R16, [specs/vendor.md](vendor.md) R15), never by joining their tables.
- **R2** The **accounts** are a fixed list of system accounts the same for every company (Q1, Q2): `SCHOOL_RECEIVABLE`, `CUSTOMER_RECEIVABLE`, `VENDOR_PAYABLE` (the party accounts), `SALES`, `SALES_RETURN`, `PURCHASE`, `PURCHASE_RETURN`, `GST_OUTPUT`, `GST_INPUT`, `CASH`, `BANK`, `CHEQUE_IN_HAND`, `DISCOUNT_ALLOWED`, `ROUND_OFF`, `OPENING_BALANCE`. Nobody adds, renames or removes an account in Phase 1; the document modules choose which accounts their postings use.
- **R3** A **ledger transaction** has: the transaction date (the document's date, R12), the financial year derived from that date (R4), an optional session (R4), the document type, document id and document number it comes from, a narration (at most 500 characters), and **at least two lines**. Each line has one account, a party (party type `SCHOOL`, `VENDOR` or `CUSTOMER` and the party's id) exactly when the account is a party account of that type, and **either** a debit **or** a credit amount greater than zero with at most two decimals. The sum of the debits equals the sum of the credits; anything else is refused. Transactions and lines are **insert-only**: never edited or deleted (BR-LED-05).
- **R4** The **financial year** of a transaction is April to March of its date (`2026-04-01` to `2027-03-31` is `2026-27`) (BR-SES-01, Q9). The **session** is not derived here: the posting document gives it (it knows the school and the order date, BR-SES-03 to BR-SES-06); opening-balance transactions have none (R6, Q10).
- **R5** **Posting capability for other modules** (D2): the document modules post through one capability of this module, inside their own request transaction: the document type, id, number and date, the session, the narration and the lines. A document type and id is posted **once**: posting it again is refused, so a retry cannot count twice. A **cancel** posts one transaction that is the exact opposite of what the document posted (every debit becomes a credit of the same amount on the same account and party, BR-LED-05); reversing twice or reversing what was never posted is refused. No other module writes ledger entries, and no module changes a party's balance any other way.
- **R6** **Opening balances** (FR-LED-01, FR-LED-02; school R7, vendor R6): when a school or a vendor is created with an opening balance above zero, or its opening balance is changed, the ledger posts an `OPENING_BALANCE` transaction for the difference, dated the business date it happened, with no session: a school's is debit `SCHOOL_RECEIVABLE` (the school) and credit `OPENING_BALANCE`; a vendor's is debit `OPENING_BALANCE` and credit `VENDOR_PAYABLE` (the vendor); a lowered balance posts the opposite (Q3, Q4). It reacts to the school's and vendor's events (school R17, vendor R16), so the master never writes the ledger. The schools and vendors that already have an opening balance when this feature arrives get their opening transaction once.
- **R7** **Balance of a party**: a school's (or customer's) balance is the sum of its debits minus the sum of its credits on its party account; a vendor's is its credits minus its debits (BR-LED-01, BR-LED-02). It may be negative (an advance). It is always computed from the lines, never stored apart from them.
- **R8** **No rounding** happens in the ledger (Q11): every amount it receives already has at most two decimals and is stored as given (`numeric(18,2)`); an amount with more decimals is refused, never rounded. Rounding is a decision of the document that computes the amount.
- **R9** **Party ledger statement** (FR-LED-03, FR-LED-04, FR-LED-06): for one school or one vendor, the lines of that party in date order (then posting order), with the columns date, document type, document number, narration, debit, credit and running balance. Filters: financial year, session (schools), a date range, or "all". The first row is the **opening balance**: the balance of every line of the party before the filter (for a session filter: every line of earlier sessions and every line without a session; for a year or date filter: every line dated before it). The answer also gives the total debit, total credit and closing balance of the filter. At most 5000 lines per answer; a wider filter is refused with a message to narrow it (Q6).
- **R10** **A new session or year needs no posting** (FR-LED-07, BR-SES-07): the opening balance of a session or year is the balance before it (R9), so the old session keeps its entries and the new one starts with that balance.
- **R11** **Outstanding list** (FR-LED-10, Q8): all schools with a non-zero balance in one list and all vendors with a non-zero balance in another, each with code, name and balance, largest first, searchable by name or code, paged (page size capped at 200), with the total of the list.
- **R12** The **transaction date** is the date of the document, not the time it was saved (an opening balance: the business date it was entered, in `Asia/Kolkata`); the time it was saved and who posted it are kept on the transaction.
- **R13** **Who reads**: the Owner and an entered Super Admin read statements and the outstanding lists. Staff have no access to the ledger (`403`, Q5); anonymous `401`. With no company entered a Super Admin reads nothing (platform P20). Nobody posts through an endpoint: postings come only from R5 and R6.
- **R14** The ledger's own history is its transactions (each names who posted it and when, R3, R12); the platform audit trail is not used for them (like the stock entries).
- **R15** Money moved through the ledger is idempotent and atomic: one document's transaction is written in the request transaction of that document, completely or not at all, and the unique posting key of R5 makes a retry harmless (CLAUDE.md "Money").
- **R16** A school's or vendor's ledger keeps its entries when the party is deactivated; the statement and the outstanding list show it under its name with an "inactive" mark.
- **R17** **Balances of a list of parties** (for the Balance column of the school and vendor lists, section 6): for at most 100 school ids (or vendor ids; 100 ids keep the request line under the server's limit) of the session's company, each one's balance by the sign of R7 (0 for a party with no line; an id of another company or unknown is left out). Read by the Owner and an entered Super Admin only (R13). Added on 2026-11-15 when the owner moved the ledger into the school and vendor profiles.

## 4. Data

Schema `finance`:
- `ledger_transaction` (aggregate root, company-owned, platform P18): `company_id`, `transaction_date` (`date`), `financial_year` (text `2026-27`), `session` (text, null), `document_type`, `document_id`, `document_number`, `posting_kind` (`POST` or `REVERSAL`), `narration`, plus the standard audit and concurrency columns. Unique on `(company_id, document_type, document_id, posting_kind)` (R5).
- `ledger_line` (insert-only child, company-owned): `company_id`, `ledger_transaction_id`, `account` (text, the list of R2, check constraint), `party_type` (null or `SCHOOL`, `VENDOR`, `CUSTOMER`), `party_id` (null or the party's id, no foreign key across schemas), `debit_amount`, `credit_amount` (`numeric(18,2)`, exactly one greater than zero), plus the transaction's date, financial year and session copied for the statement's filters. Indexes on `(company_id, party_type, party_id, transaction_date)` and `(company_id, account)`.

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session (platform P19, P20); the company is never a request field. Nothing posts through an endpoint (R13).

| Method + route | Who | Notes |
|---|---|---|
| `GET schools/{schoolId}/ledger` | `OWNER`, `SUPER_ADMIN` (entered) | `?financialYear=&session=&from=&to=` -> the statement of R9; `404` for an unknown school; `400` for more than 5000 lines |
| `GET vendors/{vendorId}/ledger` | `OWNER`, `SUPER_ADMIN` (entered) | the same for a vendor (no session filter) |
| `GET ledger/outstanding/schools` | `OWNER`, `SUPER_ADMIN` (entered) | R11, `?search=&pageIndex=&pageSize=` |
| `GET ledger/outstanding/vendors` | `OWNER`, `SUPER_ADMIN` (entered) | R11 |
| `GET ledger/balances/schools?ids=` / `GET ledger/balances/vendors?ids=` | `OWNER`, `SUPER_ADMIN` (entered) | R17: `{id: balance}` for at most 100 ids; `400` above |

The posting capability of R5 is not an endpoint. Errors are `ProblemDetails` with a message code from the module's catalogue.

## 6. User interface

Written in the UI plans; summary (owner's decisions of 2026-11-15, platform U14): there is **no Ledger menu and no separate ledger page**. A school opens beside the school list as its **profile** with a header (code, name, status and, for the Owner, its balance) and tabs: Profile, Classes and **Ledger** (the statement of R9 with its filters, opening row, running balance and totals); the later school documents (orders, invoices, payments, returns, samples) become further tabs. A vendor opens the same way with the tabs Profile and Ledger. For the Owner the school list has a **Balance** column (what the school owes) and the vendor list a **Balance** column (what the company owes), filled through R17, and a "with a balance only" filter that lists the outstanding parties of R11, largest first, with the total. Each state shows loading, empty, error with retry and success.

## 7. Events

Raised by the `LedgerTransaction` aggregate (past tense, ids and values only): `LedgerPosted` (document type and id, financial year, the parties touched). Reactions: none in this feature. This module reacts to school R17 and vendor R16 (R6).

## 8. Dependencies

`finance` depends on `iam` for the current company, on the platform (P18-P21), on `school` (directory R16, opening-balance event R17) and on `vendor` (directory R15, opening-balance event R16). School Invoice, Payment, Return, Purchase and Sample will depend on `finance` through the posting capability (R5).

## 9. Acceptance criteria

- **AC1** Through the capability (scratch caller): a document with debit `SCHOOL_RECEIVABLE` school S 1180.00 and credits `SALES` 1000.00 and `GST_OUTPUT` 180.00 dated 2026-05-10, session `2026-27` is posted: one transaction, financial year `2026-27`; S's balance goes up by 1180.00 (R3, R4, R5, R7).
- **AC2** Unbalanced lines (1180.00 against 1179.99), a line with both debit and credit, a line of 0, an amount with three decimals, a party line without a party, a party on `SALES`, and a single line are refused and nothing is written (R3, R8).
- **AC3** Posting the same document again is refused; reversing it posts one transaction with the opposite lines and S's balance is back where it was; reversing twice and reversing an unknown document are refused (R5, R15).
- **AC4** A school created with opening balance 5000.00 gets one `OPENING_BALANCE` transaction (debit S 5000.00, credit `OPENING_BALANCE`), no session; editing it to 4500.00 posts the opposite 500.00; a vendor created with 2000.00 gets credit `VENDOR_PAYABLE` 2000.00; existing schools and vendors with an opening balance each have exactly one opening transaction after the feature arrives (R6).
- **AC5** S's statement with all lines shows the opening row 0, the lines in date order with the running balance, and totals that match SQL; with financial year `2027-28` the opening row equals the balance of everything before 2027-04-01; with session `2026-27` the opening row is the opening-balance transaction (no session) (R9, R10).
- **AC6** The outstanding list of schools shows S with its balance, not a school with balance 0, sorted largest first, with the list total equal to SQL; the vendors list the same for vendors (R11).
- **AC7** The Owner of B reading A's school ledger gets `404`, identical to an unknown school; Staff get `403` on every ledger route; anonymous `401`; a Super Admin with no company entered gets empty outstanding lists (R1, R13).
- **AC8** No route changes or deletes a ledger entry (`PUT`/`DELETE` -> `405`), and SQL shows every transaction balanced (sum of debits = sum of credits per transaction) (R3, R14).
- **AC9** `GET ledger/balances/schools?ids=S,S2,<B's school>` as the Owner of A returns S and S2 with the balances of their statements and leaves B's out; 101 ids -> `400`; Staff `403` (R17).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `finance` with its own schema. `LedgerTransaction` is a company-owned aggregate root with insert-only lines; every transaction balances (CLAUDE.project.md "the ledger is double-entry"). (proposed by the assistant, Q1) |
| D2 | The document modules post through one capability of this module (R5) and never write entries themselves, like stock R11. (proposed by the assistant) |
| D3 | The session of a document is not derived in this module: school D7 already decided it comes with the first order feature; the ledger stores what the document gives (R4). (proposed by the assistant) |
| D4 | Opening balances reach the ledger through the masters' events (school R17, vendor R16), so the masters never write the ledger and the ledger stays the only writer of balances. (proposed by the assistant, Q3) |

## 11. Open questions

None. Q1-Q11 were answered by the owner one by one on 2026-11-14, all with the recommended answers; the rules above follow them. The list keeps the questions with the answer taken.

- **Q1** The kind of ledger. Recommended: real double-entry with fixed system accounts (every transaction balances; the school and vendor ledgers are the lines of their party accounts), as `CLAUDE.project.md` requires (R2, R3, D1). Alternative: only party ledgers (one debit or credit row per document, no accounts).
- **Q2** The account list. Recommended: the fixed list of R2 for Phase 1, the same for every company. Alternative: the Owner can add accounts.
- **Q3** Opening balances in the ledger. Recommended: posted automatically when a school or vendor is created or its opening balance changes, and once for the existing ones (R6, D4). Alternative: an Owner's "post opening balances" button.
- **Q4** Changing an opening balance after documents exist. Recommended: allowed; the difference is posted as a new opening transaction (the history shows both). Alternative: locked once the party has another transaction.
- **Q5** Staff and the ledger. Recommended: no access (money; R13). Alternative: Staff read the school ledger and outstanding.
- **Q6** A very long statement. Recommended: at most 5000 lines per answer, narrow the filter otherwise (R9). Alternative: paged statement.
- **Q7** PDF and Excel of a statement (FR-LED-08). Recommended: a later feature of its own (a PDF package needs approval; Excel can use ClosedXML then). Alternative: Excel now with ClosedXML.
- **Q8** The outstanding list. Recommended: only non-zero balances, largest first, with the total (R11).
- **Q9** Financial year label. Recommended: April-March of the date, written `2026-27` (R4).
- **Q10** The session of an opening balance. Recommended: none; it counts as "before" every session and year in the statement (R4, R9).
- **Q11** Rounding. Recommended: the ledger never rounds; it refuses an amount with more than two decimals (R8). The documents decide their rounding (their own question later).
