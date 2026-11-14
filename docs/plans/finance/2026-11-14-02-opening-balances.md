# Plan finance#02: Opening balances of schools and vendors in the ledger

- **File:** `docs/plans/finance/2026-11-14-02-opening-balances.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R6; AC4
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `finance#01`, `school#04`, `vendor#04`
- **UI plan:** none, nothing new on screen: the opening balance is still entered on the school or vendor; it shows in the statement of finance#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the opening balance of every school and vendor is in the ledger: a new school or vendor with an opening balance, or a changed opening balance, posts an `OPENING_BALANCE` transaction for the difference, and the schools and vendors that already had one get theirs once. Try it: create a school with opening balance 5000 and read its balance through the ledger (finance#03 shows it as a statement).

## 2. Touches / leaves out
Touches: `BookInventory.Application/Finance` (two event handlers for `SchoolOpeningBalanceChanged` and `VendorOpeningBalanceChanged`, the one-time catch-up of existing balances), `BookInventory.Api` hosting if the catch-up runs at start-up, `docs/Code_Map.md`.
Leaves out: the statement (finance#03), any UI, customers (parked).
Schema change: no (data only: the catch-up writes ledger rows; it is shown and confirmed before it runs on `book_inventory`).

## 3. Approach
Each handler builds the opening transaction of R6 (school: debit SCHOOL_RECEIVABLE / credit OPENING_BALANCE; vendor: debit OPENING_BALANCE / credit VENDOR_PAYABLE; a lowered amount the opposite) for the difference and posts it through `ILedgerPosting` with document type `OPENING_BALANCE`, a new document id per change and the party's code as number, dated the business date, no session; it runs in the school's or vendor's request transaction (a failing handler rolls the edit back). The catch-up reads every school and vendor with an opening balance whose party has no `OPENING_BALANCE` transaction yet and posts it once; it is idempotent (a second run posts nothing), so it can run at start-up like the seeders. Reading the masters goes through their directories or a read port, never their tables.

## 4. Tasks (at most 8)
- [ ] 1. Handler for `SchoolOpeningBalanceChanged`: the school's opening transaction for the difference (R6).
- [ ] 2. Handler for `VendorOpeningBalanceChanged`: the vendor's (R6).
- [ ] 3. The one-time, idempotent catch-up of existing opening balances (R6); show what it will post on `book_inventory` before it runs.
- [ ] 4. Build at 0 warnings, 0 errors; `docs/Code_Map.md` (the first `IDomainEventHandler<T>` in `src/`).
- [ ] 5. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R6`, `AC4` | 1 | School created with 5000.00 -> SQL one OPENING_BALANCE transaction, debit S 5000.00, credit OPENING_BALANCE, no session, dated today (Asia/Kolkata); edit to 4500.00 -> credit S 500.00; edit keeping 4500.00 -> nothing; created with 0 -> nothing |
| `R6`, `AC4` | 2 | Vendor created with 2000.00 -> credit VENDOR_PAYABLE 2000.00; edit to 2500.00 -> credit 500.00 more |
| `R6`, `AC4` | 3 | Before: count of schools and vendors with an opening balance; after the catch-up: exactly one opening transaction each and the balances equal their opening amounts (SQL); a second run posts nothing |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The catch-up writes data on `book_inventory` (local only; never a shared database). Rollback: delete the `OPENING_BALANCE` transactions it wrote (only with the owner's approval, as for any data change).
