# Plan payment#10: UI receipt print and download

- **File:** `docs/plans/payment/2026-11-15-10-ui-receipt-pdf.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R17 (the screen side); AC14
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** feature/payment-09 (UI repository branch feature/payment-10)
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
- [x] 1. Service call for the PDF.
- [x] 2. Print and Download buttons with loading and error states.
- [x] 3. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC14` | 1-2 | Owner prints and downloads a receipt PDF**Observed (headless Chrome, Owner of Children Book Center, scratch API on the dev proxy port, a test receipt RCP/2026-27/0028):** the receipt screen has the buttons Print and Download PDF beside the number; Print opens the new tab first and gives it a blob URL whose content is a 63,506-byte `application/pdf` starting `%PDF-` (the tab was stubbed in the page, headless Chrome does not report tabs); Download PDF creates a link named `RCP-2026-27-0028.pdf` whose content is the same 63,506 bytes starting `%PDF-`; no error is shown. UI build of 3 apps passes, lint shows only the 3 known findings. |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome driving the real screen (not a human click-through).

## Not verified
The real browser print dialog and the real downloaded file (a person should click Print and Download PDF and look at the A5 page), the error text when the PDF renderer cannot start (as sales#07 AC26), Staff not seeing the Payments tab, and the narrow screen. The test receipt was cancelled afterwards; the test school was deactivated.
