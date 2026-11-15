# Plan payment#05: Adjust an advance and cancel a receipt

- **File:** `docs/plans/payment/2026-11-15-05-advance-and-cancel.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R13-R15; AC11, AC12
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-04
- **Depends on:** `payment#01`, `payment#03`
- **UI plan:** payment#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner can apply a receipt's advance to open invoices, and can cancel a receipt (with a reason) which reverses its ledger entry and reopens its invoices.

## 2. Touches / leaves out
Touches: the `payment` module (two behaviours of `Receipt`, two endpoints), the message catalogue. No schema change.
Leaves out: screens (payment#06), the PDF.

## 3. Approach
`Receipt.AdjustAdvance` reuses the allocation rule of R6 with the advance as the amount and adds allocation rows with reason `ADJUSTED`; `Receipt.Cancel` refuses when a cheque has a result, adds the negative rows and marks the receipt `CANCELLED`; the service posts the finance reversal for the cancel.

## 4. Tasks (at most 8)
- [x] 1. Domain: adjust advance and cancel (R13, R14).
- [x] 2. Use cases: the ledger reversal on cancel; no posting on adjust.
- [x] 3. Endpoints, roles, catalogue; build at 0 warnings.
- [x] 4. Verify AC11 and AC12 on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R13`, `AC11` | 1, 2 | adjust 500 onto an open invoice -> due down by 500, no ledger transaction added**Observed (local book_inventory, scratch API):** receipt 1,500 cash with 1,000 applied by hand to the 1,000 invoice: advance 500, the 500 invoice open; Staff -> 403; an invoice that is not open -> 400 `payment.invoice_not_open`; 600 on an invoice with due 500 -> 400 `payment.invalid_allocation`; adjust (oldest first) -> 204: advance 0, allocated 1,500, nothing open, balance 0; the number of ledger transactions of the school's receipts did not change; allocation rows ALLOCATED 1000.00 (#0), ADJUSTED 500.00 (#1); adjusting again -> 409 `payment.no_advance`. |
| `R14`, `R15`, `AC12` | 1, 2 | cancel -> opposite transaction, dues return, CANCELLED with the number; a receipt with a cleared cheque -> 409**Observed (local book_inventory, scratch API):** a reason of 3 characters -> 400 `payment.invalid_reason`; Staff -> 403; cancel -> 204: status CANCELLED, the same number, reason "Entered by mistake" and date 2026-11-16 in the detail, no allocations, both invoices open again (due 1,000 and 500), balance 1,500; the receipt's ledger document went from 1 to 2 transactions (the opposite one); cancelling again -> 409 `payment.receipt_not_recorded`; adjusting a cancelled receipt -> 409; a receipt with a CLEARED cheque -> 409 `payment.receipt_has_cheque_result`, status and ledger unchanged; a receipt with a PENDING cheque cancels (balance back to 1,000); the Owner of ABC -> 404 on cancel and adjust. 18 of 18 checks passed. |

## 6. Not verified
Plan said "No schema change"; two additive nullable columns were needed on `payment.receipt`: `cancel_reason` (varchar 500) and `cancelled_on` (date, the business date of the cancel; the spec said `cancelled_at`, corrected there). Migration `20261115215004_Payment_ReceiptCancel` (`ALTER TABLE payment.receipt ADD cancel_reason character varying(500); ALTER TABLE payment.receipt ADD cancelled_on date;`) was shown before it ran and applied at the API startup to the local `book_inventory` only. After the checks, the earlier test receipts without a cheque result (11) were cancelled through the new endpoint so the receipt numbers and the school ledgers of the test schools are clean again; 6 receipts with a cleared or bounced cheque cannot be cancelled by R14 and stay recorded (backlog B103). Not exercised: adjusting an advance by hand onto two invoices; two receipts of one school adjusting the same invoice; the default `dotnet build book-inventory.slnx` (blocked by the owner's running API): the API project was built into a scratch folder: 0 warnings, 0 errors.

## 7. Risks and notes
Money plan. No migration.
