# Plan finance#02: Opening balances of schools and vendors in the ledger

- **File:** `docs/plans/finance/2026-11-14-02-opening-balances.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R6; AC4
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `finance#01`, `school#04`, `vendor#04`
- **UI plan:** none, nothing new on screen: the opening balance is still entered on the school or vendor; it shows in the statement of finance#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14
- **Base branch:** `docs/finance-ledger` (one branch `feature/finance` for platform#08, school#03, school#04, vendor#04 and finance#01-#03, the owner's instruction "all plan done kro")

## 1. In one minute
After this plan the opening balance of every school and vendor is in the ledger: a new school or vendor with an opening balance, or a changed opening balance, posts an `OPENING_BALANCE` transaction for the difference, and the schools and vendors that already had one get theirs once. Try it: create a school with opening balance 5000 and read its balance through the ledger (finance#03 shows it as a statement).

## 2. Touches / leaves out
Touches: `BookInventory.Application/Finance` (two event handlers for `SchoolOpeningBalanceChanged` and `VendorOpeningBalanceChanged`, the one-time catch-up of existing balances), `BookInventory.Api` hosting if the catch-up runs at start-up, `docs/Code_Map.md`.
Leaves out: the statement (finance#03), any UI, customers (parked).
Schema change: no (data only: the catch-up writes ledger rows; it is shown and confirmed before it runs on `book_inventory`).

## 3. Approach
Each handler builds the opening transaction of R6 (school: debit SCHOOL_RECEIVABLE / credit OPENING_BALANCE; vendor: debit OPENING_BALANCE / credit VENDOR_PAYABLE; a lowered amount the opposite) for the difference and posts it through `ILedgerPosting` with document type `OPENING_BALANCE`, a new document id per change and the party's code as number, dated the business date, no session; it runs in the school's or vendor's request transaction (a failing handler rolls the edit back). The catch-up reads every school and vendor with an opening balance whose party has no `OPENING_BALANCE` transaction yet and posts it once; it is idempotent (a second run posts nothing), so it can run at start-up like the seeders. Reading the masters goes through their directories or a read port, never their tables.

## 4. Tasks (at most 8)
- [x] 1. Handler for `SchoolOpeningBalanceChanged`: the school's opening transaction for the difference (R6).
- [x] 2. Handler for `VendorOpeningBalanceChanged`: the vendor's (R6).
- [x] 3. The one-time, idempotent catch-up of existing opening balances (R6); show what it will post on `book_inventory` before it runs.
- [x] 4. Build at 0 warnings, 0 errors; `docs/Code_Map.md` (the first `IDomainEventHandler<T>` in `src/`).
- [x] 5. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R6`, `AC4` | 1 | School S2 created with 5000.00 -> SQL `SCHOOL_RECEIVABLE:SCHOOL:5000.00:0.00, OPENING_BALANCE:-:0.00:5000.00`, no session, dated 2026-11-14 (Asia/Kolkata), number = the school's code; edit to 4500.00 -> a second transaction, balance 4500.00; edit keeping it -> none; created with 0 -> none. **PASS** |
| `R6`, `AC4` | 2 | Vendor V created with 2000.00 -> `VENDOR_PAYABLE:0.00:2000.00, OPENING_BALANCE:2000.00:0.00`; edit to 2500.00 -> balance 2500.00. **PASS** |
| `R6`, `AC4` | 3 | Before: 0 schools and 1 vendor of `book_inventory` had an opening balance; after the catch-up: 1 system opening line for that vendor with its amount, 0 parties with an opening balance left without one (so a second run posts nothing), and for every live school and vendor the opening lines equal its opening balance (SQL mismatch count 0 and 0). **PASS** |

## 6. Not verified
Run by `check_finance.py` (scratch, not committed; a temporary scratch controller and a throw-away failing handler were deleted before the commit): 28/28 passed on the real `book_inventory`.

The catch-up is the data migration `Finance_OpeningBalanceCatchUp` (insert-only; its SQL was shown in the session; target the local `book_inventory`; it failed once on a uuid cast before it was applied, was corrected and then applied). It is SQL, not a start-up job, because a job outside a request has no company to stamp (platform P20); its accounts repeat the rule of `OpeningBalanceEntry` once, for this one-time snapshot. Rollback: none in the migration (removing posted ledger entries is a data change the owner approves on its own).

## 7. Risks and notes
The catch-up writes data on `book_inventory` (local only; never a shared database). Rollback: delete the `OPENING_BALANCE` transactions it wrote (only with the owner's approval, as for any data change).
