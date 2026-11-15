# Plan payment#03: Cheque clear and bounce

- **File:** `docs/plans/payment/2026-11-15-03-cheque-status.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R10-R12; AC8-AC10
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-02
- **Depends on:** `payment#01`
- **UI plan:** payment#04
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a cheque can be marked cleared or bounced; a bounce puts the amount back on the school's balance and reopens the invoices it had paid; cancelling an invoice turns its payments into advance.

## 2. Touches / leaves out
Touches: the `payment` module (the cheque behaviours of `Receipt`, the reaction to the sales `InvoiceCancelled` event, two endpoints), the message catalogue. No schema change (the columns exist from payment#01).
Leaves out: adjust and cancel (payment#05), screens.

## 3. Approach
`Receipt.ClearCheque` and `Receipt.BounceCheque` validate the status and dates, raise events and add the signed allocation rows of R11; the service posts `CHEQUE_CLEARED` or `CHEQUE_BOUNCED` through finance R5. A handler of `InvoiceCancelled` adds the negative allocation rows (R12) in the same transaction.

## 4. Tasks (at most 8)
- [x] 1. Domain: cheque status behaviours and the bounce allocation order (R10, R11).
- [x] 2. Clear and bounce use cases with their ledger postings (R10).
- [x] 3. The handler of the invoice cancel (R12).
- [x] 4. Endpoints, roles, catalogue; build at 0 warnings.
- [x] 5. Verify AC8-AC10 with exact numbers on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `AC8` | 1, 2 | clear 10,000 -> debit BANK / credit CHEQUE_IN_HAND, CLEARED; again -> 409**Observed (local book_inventory, scratch API):** cheque 1000 paid the 1000 invoice; clearing before the cheque date and in the future -> 400; clear on 2026-11-12 -> 204, CLEARED; ledger CHEQUE_CLEARED debit BANK 1000.00 / credit CHEQUE_IN_HAND 1000.00; clearing again -> 409 `payment.cheque_result_set`; a cleared cheque cannot bounce (409); the school balance stayed 0. |
| `R10`, `R11`, `AC9` | 1, 2 | bounce with 3,000 advance -> advance 0, 7,000 reopened on the newest invoice, balance +10,000**Observed (local book_inventory, scratch API):** receipt 5000 (cheque 3000 + cash 2000) by hand on two 2000 invoices: allocated 4000, advance 1000, balance -1000; a 3-character reason -> 400 `payment.invalid_reason`; Staff -> 403; an unknown cheque -> 404; bounce -> 204, BOUNCED: the advance 1000 went first, then 2000 from the newest invoice: invoice 1 keeps 2000, invoice 2 has 0, advance 0; balance +2000 and only invoice 2 open with due 2000; ledger CHEQUE_BOUNCED debit SCHOOL_RECEIVABLE 3000.00 / credit CHEQUE_IN_HAND 3000.00, narration `RCP/2026-27/0010: cheque 778899 bounced: Insufficient funds`; allocation rows ALLOCATED 2000 (#0), ALLOCATED 2000 (#1), BOUNCED -2000 (#2). A second case, 2000 against two 1000 invoices with a 1500 cheque and no advance: the bounce emptied the newest invoice (1000) and took 500 from the older one (keeps 500); balance +1500, dues 500 and 1000. |
| `R12`, `AC10` | 3 | invoice with 800 allocated is cancelled -> 800 advance, receipt unchanged**Observed (local book_inventory, scratch API):** receipt cash 800 applied to a 1000 invoice (due 200); cancelling the invoice -> 204: the receipt shows allocated 0, advance 800, no allocations, still RECORDED with its one ledger transaction (CASH 800 / SCHOOL_RECEIVABLE credit 800); school balance -800, nothing open; invoice-dues of the cancelled invoice: due 0. 26 of 26 checks passed. |

## 6. Not verified
Plan said "No schema change"; one additive column was needed: `payment.receipt_allocation.sequence` (integer, default 0), the place of an allocation row in its receipt's history, so that "the most recently allocated invoice" (R11) is exact after a reload (UUID v7 ids of rows made in the same millisecond cannot order them). Migration `20261115214218_Payment_AllocationSequence` (`ALTER TABLE payment.receipt_allocation ADD sequence integer NOT NULL DEFAULT 0`) was shown before it ran and applied at the API startup to the local `book_inventory` only; the allocation rows of the earlier test receipts all have sequence 0. Not exercised: a bounce on a cancelled receipt (receipts cannot be cancelled until payment#05); two cheques bouncing on one receipt; the default `dotnet build book-inventory.slnx` (blocked by the owner's running API) - the API project was built into a scratch folder: 0 warnings, 0 errors.

## 7. Risks and notes
Money plan. No migration.
