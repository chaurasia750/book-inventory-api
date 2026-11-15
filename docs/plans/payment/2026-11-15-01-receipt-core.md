# Plan payment#01: Record a school payment (receipt, modes, allocation, ledger)

- **File:** `docs/plans/payment/2026-11-15-01-receipt-core.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R1-R9, R16 (list and detail), R18, R19; AC1-AC7, AC13
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `sales#14`, `finance#01` (Done), `school#01` (Done)
- **UI plan:** payment#02
- **Approved by / on:** -
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can record a payment from a school: `GET /api/v1/payment/dues?schoolId=` shows the balance and open invoices, `POST /api/v1/receipts` records cash, UPI, bank transfer or cheque modes, applies them oldest invoice first (or as given) and credits the school ledger.

## 2. Touches / leaves out
Touches: the new `payment` module (Domain, Application, Infrastructure, controllers), a new schema `payment` with one migration (`receipt`, `receipt_mode`, `receipt_allocation`), the message catalogue, module registration. Reads sales (sales#14), school directory and finance balance; posts through finance R5.
Leaves out: cheque results (payment#03), adjust advance and cancel (payment#05), the PDF (payment#07), screens.

## 3. Approach
The `Receipt` aggregate validates its modes and total, takes the allocation (oldest first or given, against the dues the application service reads), raises `ReceiptRecorded`; the service posts the ledger transaction in the same request transaction. The number comes from the company code series; the `clientRequestId` has a unique index so a retry returns the same receipt. The migration SQL is shown before it is applied to the local `book_inventory`.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `Receipt`, `ReceiptMode`, `ReceiptAllocation`, value rules for modes, money and allocation (R4-R6).
- [ ] 2. Persistence: configurations, schema `payment`, migration (show the SQL, apply to the local `book_inventory`).
- [ ] 3. Dues read: balance and open invoices (R3).
- [ ] 4. Record a receipt: number, idempotency, allocation, ledger posting (R6-R9).
- [ ] 5. Reading: list, detail, invoice dues (R16).
- [ ] 6. Controllers, roles, message catalogue; build at 0 warnings.
- [ ] 7. Verify AC1-AC7 and AC13 on the local database with exact numbers.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `AC1` | 3 | `GET payment/dues` -> balance 1,500.00 and two open invoices oldest first |
| `R6`, `R7`, `AC2` | 1, 4 | receipt 200 cash + 1,000 UPI -> invoice 1 paid, invoice 2 due 300, balance 300, ledger credit 1,200 / debit CASH 200, BANK 1,000 |
| `R6`, `AC3`, `AC4` | 1, 4 | 2,000 -> 500 advance, balance -500; hand allocations and the refusals |
| `R9`, `AC5` | 4 | same `clientRequestId` twice -> 201 then 200, one transaction |
| `R4`, `R5`, `AC6` | 1, 4 | each invalid input -> 400 with its code |
| `R8`, `R19`, `AC7` | 4 | numbers per financial year |
| `R1`, `R2`, `AC13` | 6 | Staff 403; other company 404 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Schema change: the first schema-changing plan of `payment`. Money plan: every figure above is worked out by hand before the run. Rollback note: the new schema can be dropped as a whole because nothing else references it.
