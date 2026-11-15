# Plan payment#05: Adjust an advance and cancel a receipt

- **File:** `docs/plans/payment/2026-11-15-05-advance-and-cancel.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R13-R15; AC11, AC12
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Domain: adjust advance and cancel (R13, R14).
- [ ] 2. Use cases: the ledger reversal on cancel; no posting on adjust.
- [ ] 3. Endpoints, roles, catalogue; build at 0 warnings.
- [ ] 4. Verify AC11 and AC12 on the local database.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R13`, `AC11` | 1, 2 | adjust 500 onto an open invoice -> due down by 500, no ledger transaction added |
| `R14`, `R15`, `AC12` | 1, 2 | cancel -> opposite transaction, dues return, CANCELLED with the number; a receipt with a cleared cheque -> 409 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Money plan. No migration.
