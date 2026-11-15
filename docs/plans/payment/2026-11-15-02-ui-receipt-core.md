# Plan payment#02: UI receive a payment

- **File:** `docs/plans/payment/2026-11-15-02-ui-receipt-core.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R3-R6, R16 (the screen side); AC1-AC4
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Service and models (dues, receipts, invoice dues).
- [ ] 2. The Payments tab of the school profile: list and empty/error states.
- [ ] 3. The receive form: balance, open invoices, mode rows, preview of the allocation, by-hand allocation, advance.
- [ ] 4. The receipt detail; invoice screens show allocated and due.
- [ ] 5. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `R4`-`R6`, `AC1`-`AC4` | 1-4 | Owner opens a school, Receive payment: balance and invoices show; cash + UPI is recorded and the invoices update; an advance shows; Staff does not see the tab |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
