# Plan finance#03: Ledger statement and outstanding lists

- **File:** `docs/plans/finance/2026-11-14-03-ledger-statement.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R1, R9-R11, R13, R16; AC5-AC7
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `finance#01`, `school#03` (Approved)
- **UI plan:** finance#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14
- **Base branch:** `docs/finance-ledger` (one branch `feature/finance` for platform#08, school#03, school#04, vendor#04 and finance#01-#03, the owner's instruction "all plan done kro")

## 1. In one minute
After this plan the Owner can read a school's or vendor's ledger: the opening row, every entry in date order with the running balance, and the totals, filtered by financial year, session or dates; and the outstanding lists of all schools and all vendors with a balance. Try it: `GET /api/v1/schools/{id}/ledger?financialYear=2026-27` and `GET /api/v1/ledger/outstanding/schools`.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Finance` (the statement and outstanding query service and its contracts), `BookInventory.Infrastructure/Finance` (the queries over the lines), `BookInventory.Api/Controllers/Finance` (four GET routes), the message catalogue, `docs/Code_Map.md`.
Leaves out: PDF and Excel (Q7), the "Pending" cheque mark (Payment module), opening a document from a row (later), customers (parked), any UI (finance#04).
Schema change: no.

## 3. Approach
The statement reads the party's lines in one filtered query plus one sum for the opening row (R9): for a session filter the opening row is every line of an earlier session (session text compares in order: `2026-27` before `2027-28`, `2026` before `2027`) and every line without a session; for a year or date filter every line dated earlier. The running balance and the totals are computed from the rows with the sign of R7. A party is checked through the school or vendor directory first (unknown or another company's -> `404`); more than 5000 lines -> `400`. The outstanding lists group the lines by party, keep non-zero balances, page and sort them, and add names and codes through the directories (inactive marked, R16).

## 4. Tasks (at most 8)
- [x] 1. Contracts: statement (opening row, lines with running balance, totals) and outstanding rows (R9, R11).
- [x] 2. Statement query with the filters and the opening row (R9, R10).
- [x] 3. Outstanding queries for schools and vendors with search, paging and the total (R11, R16).
- [x] 4. Controllers with the roles of R13 (Owner and entered Super Admin; Staff `403`).
- [x] 5. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 6. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R9`, `R10`, `AC5` | 2 | S all: opening 0, rows 2026-05-10 +1180.00 (1180.00), 2026-06-01 reversal -1180.00 (0.00), 2027-03-31 +10.00 (10.00), 2027-04-01 +10.00 (20.00); totals = SQL `1200.00|1180.00`; year 2027-28: opening 10.00, 1 row, closing 20.00; dates 2026-06-01..2027-03-31: opening 1180.00, 2 rows, closing 10.00; S2 session 2026-27: opening 4500.00 (the opening balance, no session), closing 4600.00; session 2027-28: opening 4600.00, closing 4650.00; vendor statement 2000.00, 2500.00. Invalid filters (year `2026-28`, year with session, from after to, a vendor's session) -> 400 `finance.invalid_filter`. **PASS** |
| `R9` | 2 | **NOT verified**: a filter above 5000 lines was not generated on the local data (the code path refuses at 5001 lines). |
| `R11`, `R16`, `AC6` | 3 | Outstanding schools (page size 200): S 20.00 and S2 4650.00 listed, the zero school absent, balances largest first, total 13710.00 = SQL 13710.00; search by S2's code -> only S2; vendors: V with 2500.00; S deactivated -> still listed, `isActive: false`. **PASS** |
| `R1`, `R13`, `AC7` | 4 | Owner of B on A's school and on an unknown id -> 404, 404 `finance.party_not_found`; Staff -> 403 on all four routes; anonymous 401; Super Admin with no company -> 200 empty list. **PASS** |

## 6. Not verified
The 5000-line refusal of R9 (row above). Run by `check_finance.py` (scratch, not committed; a temporary scratch controller and a throw-away failing handler were deleted before the commit): 28/28 passed on the real `book_inventory`.

Notes: the vendor directory (vendor R15) gained `ReferencesAsync` and the code in `VendorReference`, so the outstanding list shows vendor codes without joining the vendor tables (backlog item).

## 7. Risks and notes
The session order relies on the session text format of BR-SES (`2026-27`, `2027`); if the order feature chooses another format, this comparison changes with it.
