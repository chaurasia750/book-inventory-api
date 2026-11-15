# Plan payment#02: UI receive a payment

- **File:** `docs/plans/payment/2026-11-15-02-ui-receipt-core.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R3-R6, R16 (the screen side); AC1-AC4
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-01 (UI repository branch feature/payment-02)
- **Depends on:** `payment#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the school profile has a Payments tab and a Receive payment button: the form shows the balance and open invoices, takes the modes, shows how the money will be applied and the advance, and records the receipt. Invoice screens show allocated and due.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/school` tab, `pages/payment` form and list, invoice screens), the payment service and models, i18n.
Leaves out: cheque results, adjust, cancel, the PDF.

## 3. Approach
The panel beside the list (platform U14). Loading shows skeletons; empty shows "No payments yet"; an error shows the catalogue text of the code; success opens the new receipt. Nothing is computed in the browser except the preview of the oldest-first application, which the server repeats.

## 4. Tasks (at most 8)
- [x] 1. Service and models (dues, receipts, invoice dues).
- [x] 2. The Payments tab of the school profile: list and empty/error states.
- [x] 3. The receive form: balance, open invoices, mode rows, preview of the allocation, by-hand allocation, advance.
- [x] 4. The receipt detail; invoice screens show allocated and due.
- [x] 5. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `R4`-`R6`, `AC1`-`AC4` | 1-4 | Owner opens a school, Receive payment: balance and invoices show; cash + UPI is recorded and the invoices update; an advance shows; Staff does not see the tab**Observed (headless Chrome, Owner of Children Book Center, scratch API on the dev proxy port, a test school with invoices 1,000 and 500):** the school profile tabs are Profile, Orders, Invoices, Payments, Book lists, Ledger, Classes; the Payments tab shows the empty state "No payments yet." and the Receive payment button; the dialog (960 px wide) shows Balance 1,500.00 and the two open invoices with due and applied columns; typing 1,200 in the cash amount shows Total 1,200.00, Applied 1,200.00, Advance 0.00; Record payment answers "Payment recorded: RCP/2026-27/0008."; the list then shows that receipt (Recorded, 1,200.00); its detail shows Cash 1,200.00 applied to INV/2026-27/0023 1,000.00 and INV/2026-27/0024 200.00, advance 0.00; the invoice screen of INV/2026-27/0024 shows "Paid: 200.00 · Due: 300.00". UI build of 3 apps passes, lint shows only the 3 known findings. |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome driving the real screens (not a human click-through).

## Not verified
The by-hand allocation, an advance (a payment above the dues), a UPI or cheque mode with its fields, the error texts, a retry after a failure and Staff not seeing the tab were not clicked through; the allocations of a receipt are listed in the server's order, not by invoice number. Owner to check (backlog).
