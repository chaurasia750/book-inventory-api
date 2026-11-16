# Plan payment#11: Dashboard figures of payments

- **File:** `docs/plans/payment/2026-11-16-11-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R4, R5, R6 (the payment side); AC1, AC2
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories)
- **Depends on:** `payment#01` to `payment#10` (Done), `sales#29` is not needed
- **UI plan:** none, a read capability for the report module
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q4 and the dashboard choices answered one by one; "all approve")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the report module can ask payment for what was received in a date window by way of payment, the pending cheques, and the net allocated in the window to invoices of earlier sessions.

## 2. Touches / leaves out
Touches: `Application/Payment` (a figures capability, its answer records) and `Infrastructure/Payment` (aggregate queries on receipts, modes and allocations, with the invoice session through the sales invoice directory), `docs/Code_Map.md`. Leaves out: the screens, any change to a receipt.

## 3. Approach
`IPaymentFigures` with `ReceivedAsync(window)`, `PendingChequesAsync()` and `EarlierSessionRecoveryAsync(window, startYear)`; money is summed in the database; the earlier-session test uses the invoice's session start year read through the sales invoice directory (sales R37), never a join across schemas.

## 4. Tasks (at most 8)
- [ ] 1. The capability and its answer records (R4-R6).
- [ ] 2. Received by way of payment: Cash, UPI and Bank by receipt date, cheques by clearing date, cancelled and bounced out (R4).
- [ ] 3. Pending cheques, any date (R5).
- [ ] 4. Earlier-session recovery: signed allocation rows made in the window to earlier-session invoices, the schools and the amount still due (R6).
- [ ] 5. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 6. Verify AC1 and AC2 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R4`, `R5`, `AC1` | 2, 3 | Cash 5,000 + UPI 2,000 + cleared cheque 3,000 = 10,000 and a pending cheque of 1,000 counted apart; a bounced cheque and a cancelled receipt count for nothing |
| `R6`, `AC2` | 4 | 4,000 allocated to a `2025-26` invoice and 1,500 to a `2026-27` one: 4,000; after the receipt is cancelled 0 |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Q2 (the allocation date) was answered by abhishek on 2026-11-16: the day the allocation row was made.
