# Plan finance#03: Ledger statement and outstanding lists

- **File:** `docs/plans/finance/2026-11-14-03-ledger-statement.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R1, R9-R11, R13, R16; AC5-AC7
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `finance#01`, `school#03` (Approved)
- **UI plan:** finance#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner can read a school's or vendor's ledger: the opening row, every entry in date order with the running balance, and the totals, filtered by financial year, session or dates; and the outstanding lists of all schools and all vendors with a balance. Try it: `GET /api/v1/schools/{id}/ledger?financialYear=2026-27` and `GET /api/v1/ledger/outstanding/schools`.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Finance` (the statement and outstanding query service and its contracts), `BookInventory.Infrastructure/Finance` (the queries over the lines), `BookInventory.Api/Controllers/Finance` (four GET routes), the message catalogue, `docs/Code_Map.md`.
Leaves out: PDF and Excel (Q7), the "Pending" cheque mark (Payment module), opening a document from a row (later), customers (parked), any UI (finance#04).
Schema change: no.

## 3. Approach
The statement reads the party's lines in one filtered query plus one sum for the opening row (R9): for a session filter the opening row is every line of an earlier session (session text compares in order: `2026-27` before `2027-28`, `2026` before `2027`) and every line without a session; for a year or date filter every line dated earlier. The running balance and the totals are computed from the rows with the sign of R7. A party is checked through the school or vendor directory first (unknown or another company's -> `404`); more than 5000 lines -> `400`. The outstanding lists group the lines by party, keep non-zero balances, page and sort them, and add names and codes through the directories (inactive marked, R16).

## 4. Tasks (at most 8)
- [ ] 1. Contracts: statement (opening row, lines with running balance, totals) and outstanding rows (R9, R11).
- [ ] 2. Statement query with the filters and the opening row (R9, R10).
- [ ] 3. Outstanding queries for schools and vendors with search, paging and the total (R11, R16).
- [ ] 4. Controllers with the roles of R13 (Owner and entered Super Admin; Staff `403`).
- [ ] 5. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R9`, `R10`, `AC5` | 2 | School S with opening 5000.00 and scratch postings in 2026-27 and 2027-28: all -> opening 0, rows in date order, running balance, totals = SQL; year 2027-28 -> opening = balance before 2027-04-01; session 2026-27 -> opening = the opening balance; dates -> opening before `from` |
| `R9` | 2 | A filter with more than 5000 lines -> 400 (generated scratch postings) or recorded as not reachable on the local data |
| `R11`, `R16`, `AC6` | 3 | Outstanding schools: S with its balance, a school with 0 absent, largest first, total = SQL, search by code; vendors the same; an inactive school marked |
| `R1`, `R13`, `AC7` | 4 | Owner of B on A's school -> 404 identical to unknown; Staff on all four routes -> 403; anonymous 401; Super Admin without company -> empty lists |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The session order relies on the session text format of BR-SES (`2026-27`, `2027`); if the order feature chooses another format, this comparison changes with it.
