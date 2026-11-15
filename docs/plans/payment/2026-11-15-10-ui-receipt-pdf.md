# Plan payment#10: UI receipt print and download

- **File:** `docs/plans/payment/2026-11-15-10-ui-receipt-pdf.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R17 (the screen side); AC14
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `payment#09`, `payment#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: "yes all approve hai"; Q1-Q7 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the receipt screen has Print and Download PDF buttons.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/payment`), service, i18n.
Leaves out: nothing else.

## 3. Approach
The same two actions as the invoice screen: open the PDF in the browser print dialog, or download it; an error shows the catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. Service call for the PDF.
- [ ] 2. Print and Download buttons with loading and error states.
- [ ] 3. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC14` | 1-2 | Owner prints and downloads a receipt PDF |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
