# Plan payment#11: Dashboard figures of payments

- **File:** `docs/plans/payment/2026-11-16-11-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R4, R5, R6 (the payment side); AC1, AC2
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
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
- [x] 1. The capability and its answer records (R4-R6).
- [x] 2. Received by way of payment: Cash, UPI and Bank by receipt date, cheques by clearing date, cancelled and bounced out (R4).
- [x] 3. Pending cheques, any date (R5).
- [x] 4. Earlier-session recovery: signed allocation rows made in the window to earlier-session invoices, the schools and the amount still due (R6).
- [x] 5. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 6. Verify AC1 and AC2 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R4`, `R5`, `AC1` | 2, 3 | Cash 5,000 + UPI 2,000 + cleared cheque 3,000 = 10,000 and a pending cheque of 1,000 counted apart; a bounced cheque and a cancelled receipt count for nothing ; receipt of Cash 600 + UPI 400 + cheque 1000 (pending): Cash +600, UPI +400, Cheque +0, received +1000, one receipt more, pending cheque +1 / +1000; after the cheque cleared today Cheque +1000, received +2000 in all, pending back; a second receipt that was cancelled counts for nothing (total and receipts as before) |
| `R6`, `AC2` | 4 | 4,000 allocated to a `2025-26` invoice and 1,500 to a `2026-27` one: 4,000; after the receipt is cancelled 0 ; 1000 allocated to an invoice of `2025-26` and 1000 to a `2026-27` one: the earlier-session amount +1000, one school, and the amount still due on those invoices fell by 1000; a receipt of 500 on another earlier-session invoice +500, and after it was cancelled the amount was as before |

## 6. Not verified
Build: `dotnet build src/BookInventory.Api` to a scratch output (the owner's own API holds the default `bin`) -> 0 warnings, 0 errors.

Scratch check on the real `book_inventory` (scratch API on 7900, owner login): 31 checks, all passed; the dashboard was read before and after each step and the deltas compared with figures worked out first.

Deviation from the plan: the capability does not depend on sales; the report module gives it the invoice ids (`AllocationsAsync(invoiceIds, from, to)`), and the allocation's business date (Q2) is read from its UUID v7 id because the row keeps no date column (no migration).

NOT verified: a bounced cheque (counts for nothing and reopens the invoice), a cheque cleared outside the window, Bank transfer, the window edges, the speed with many thousands of allocation rows (they are read into memory for the earlier-session invoices).

## 7. Risks and notes
Q2 (the allocation date) was answered by abhishek on 2026-11-16: the day the allocation row was made.
