# Plan payment#03: Cheque clear and bounce

- **File:** `docs/plans/payment/2026-11-15-03-cheque-status.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R10-R12; AC8-AC10
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Domain: cheque status behaviours and the bounce allocation order (R10, R11).
- [ ] 2. Clear and bounce use cases with their ledger postings (R10).
- [ ] 3. The handler of the invoice cancel (R12).
- [ ] 4. Endpoints, roles, catalogue; build at 0 warnings.
- [ ] 5. Verify AC8-AC10 with exact numbers on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `AC8` | 1, 2 | clear 10,000 -> debit BANK / credit CHEQUE_IN_HAND, CLEARED; again -> 409 |
| `R10`, `R11`, `AC9` | 1, 2 | bounce with 3,000 advance -> advance 0, 7,000 reopened on the newest invoice, balance +10,000 |
| `R12`, `AC10` | 3 | invoice with 800 allocated is cancelled -> 800 advance, receipt unchanged |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Money plan. No migration.
