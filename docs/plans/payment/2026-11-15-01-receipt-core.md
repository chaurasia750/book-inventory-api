# Plan payment#01: Record a school payment (receipt, modes, allocation, ledger)

- **File:** `docs/plans/payment/2026-11-15-01-receipt-core.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R1-R9, R16 (list and detail), R18, R19; AC1-AC7, AC13
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/sales-14
- **Depends on:** `sales#14`, `finance#01` (Done), `school#01` (Done)
- **UI plan:** payment#02
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can record a payment from a school: `GET /api/v1/payment/dues?schoolId=` shows the balance and open invoices, `POST /api/v1/receipts` records cash, UPI, bank transfer or cheque modes, applies them oldest invoice first (or as given) and credits the school ledger.

## 2. Touches / leaves out
Touches: the new `payment` module (Domain, Application, Infrastructure, controllers), a new schema `payment` with one migration (`receipt`, `receipt_mode`, `receipt_allocation`), the message catalogue, module registration. Reads sales (sales#14), school directory and finance balance; posts through finance R5.
Leaves out: cheque results (payment#03), adjust advance and cancel (payment#05), the PDF (payment#07), screens.

## 3. Approach
The `Receipt` aggregate validates its modes and total, takes the allocation (oldest first or given, against the dues the application service reads), raises `ReceiptRecorded`; the service posts the ledger transaction in the same request transaction. The number comes from the company code series; the `clientRequestId` has a unique index so a retry returns the same receipt. The migration SQL is shown before it is applied to the local `book_inventory`.

## 4. Tasks (at most 8)
- [x] 1. Domain: `Receipt`, `ReceiptMode`, `ReceiptAllocation`, value rules for modes, money and allocation (R4-R6).
- [x] 2. Persistence: configurations, schema `payment`, migration (show the SQL, apply to the local `book_inventory`).
- [x] 3. Dues read: balance and open invoices (R3).
- [x] 4. Record a receipt: number, idempotency, allocation, ledger posting (R6-R9).
- [x] 5. Reading: list, detail, invoice dues (R16).
- [x] 6. Controllers, roles, message catalogue; build at 0 warnings.
- [x] 7. Verify AC1-AC7 and AC13 on the local database with exact numbers.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `AC1` | 3 | `GET payment/dues` -> balance 1,500.00 and two open invoices oldest first**Observed (local book_inventory, scratch API):** school with invoices 1,000 (INV/2026-27/0016) and 500 (0017): balance 1500.0, two open invoices oldest first, dues 1000 and 500, allocated 0. |
| `R6`, `R7`, `AC2` | 1, 4 | receipt 200 cash + 1,000 UPI -> invoice 1 paid, invoice 2 due 300, balance 300, ledger credit 1,200 / debit CASH 200, BANK 1,000**Observed (local book_inventory, scratch API):** receipt 200 cash + 1,000 UPI (UPI12345) -> 201 `RCP/2026-27/0001`, total 1200, advance 0; invoice 1 allocated 1000, invoice 2 allocated 200; balance 300.0, open: the 500 invoice with due 300; ledger lines CREDIT SCHOOL_RECEIVABLE 1200.00, DEBIT CASH 200.00, DEBIT BANK 1000.00; narration `RCP/2026-27/0001: Cash 200.00, UPI 1000.00`; invoice-dues 0 and 300. |
| `R6`, `AC3`, `AC4` | 1, 4 | 2,000 -> 500 advance, balance -500; hand allocations and the refusals**Observed (local book_inventory, scratch API):** AC3: 2,000 cash against 1,500 -> allocated 1500, advance 500, balance -500.0, nothing open. AC4: 1,000 with 300 by hand to the 500 invoice -> accepted, advance 700; 600 to an invoice with due 200 -> 400 `payment.invalid_allocation`; an invoice of another school and a cancelled invoice -> 400 `payment.invoice_not_open`; 300 allocated of a 100 payment -> 400 `payment.invalid_allocation`. |
| `R9`, `AC5` | 4 | same `clientRequestId` twice -> 201 then 200, one transaction**Observed (local book_inventory, scratch API):** same `clientRequestId` twice -> 201 then 200 with the same receipt id; one ledger transaction; one receipt row added. |
| `R4`, `R5`, `AC6` | 1, 4 | each invalid input -> 400 with its code**Observed (local book_inventory, scratch API):** cheque without number / date / bank -> 400 `payment.invalid_cheque`; UPI without id and a 2-character UTR -> 400 `payment.invalid_reference`; amount 0 and 10.123 -> 400 `payment.invalid_amount`; 11 modes and no modes -> 400 `payment.invalid_modes`; date 2099-01-01 -> 400 `payment.invalid_date`; a missing `clientRequestId` -> 400; an unknown school -> 400 `payment.school_not_found`; a cheque of 100 is accepted PENDING and posts CREDIT SCHOOL_RECEIVABLE 100 / DEBIT CHEQUE_IN_HAND 100. |
| `R8`, `R19`, `AC7` | 4 | numbers per financial year**Observed (local book_inventory, scratch API):** dates 2026-04-01 and 2026-04-02 -> `RCP/2026-27/0006` and `0007`; dates 2026-03-31 and 2026-03-30 (a receipt cannot be dated in the future, so the year before stands for the restart) -> `RCP/2025-26/0001` and `0002`. |
| `R1`, `R2`, `AC13` | 6 | Staff 403; other company 404**Observed (local book_inventory, scratch API):** Staff -> 403 on all five routes tried, no token -> 401; the Owner of ABC Book Center -> 404 on this company's receipt and on its school's dues, and an empty list. 39 of 39 checks passed. |

## 6. Not verified
The list is searchable by receipt number and filterable by school, status, dates, mode and cheque status; searching by the school's name or code (R16) needs a school search in the school directory and is not built (backlog); the school's Payments tab uses the school filter. The receipts the check recorded (`RCP/2026-27/0001`-`0007`, `RCP/2025-26/0001`-`0002`, on deactivated test schools of Children Book Center) cannot be cancelled before payment#05; they are listed in the backlog. The default `dotnet build book-inventory.slnx` was blocked by the owner's running API (file lock); the API project was built into a scratch folder and with `--configuration Release` for the migration: 0 warnings, 0 errors. Migration `20261115213022_Payment_Receipts` (additive: schema `payment`, 3 tables) was shown before it ran and was applied at the API startup to the local `book_inventory` only.

## 7. Risks and notes
Schema change: the first schema-changing plan of `payment`. Money plan: every figure above is worked out by hand before the run. Rollback note: the new schema can be dropped as a whole because nothing else references it.
