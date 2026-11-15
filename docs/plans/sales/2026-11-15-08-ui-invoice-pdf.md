# Plan sales#08: UI invoice PDF

- **File:** `docs/plans/sales/2026-11-15-08-ui-invoice-pdf.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R32, R33 (the screen side); AC24
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `sales#07`, `sales#04`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (the invoice PDF approval, Q15-Q20 with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (to be cut as `feature/sales-ui-08` when approved)

## 1. In one minute
After this plan the invoice detail shows the GST the same way the printed PDF does: a **rate-wise summary** (no per-line GST column), the totals and the amount in words, with **Print** and **Download PDF** actions. Open a school, tab Invoices, open an invoice.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/sales/invoices`: the invoice view — the rate-wise summary block, the amount in words, the Print and Download PDF actions), the service and models, i18n.
Leaves out: the PDF generation itself (sales#07); the make-invoice and cancel screens (sales#04).

## 3. Approach
The invoice view drops any per-line GST column and shows one **rate-wise summary** table (HSN/rate, taxable value, CGST + SGST or IGST, tax total), the totals and the amount in words, matching the PDF exactly (R32, R33); with GST off the summary and the tax rows are not shown. **Download PDF** opens `GET invoices/{id}/pdf` (with the current layout); **Print** opens the PDF in a new tab for the browser's print dialog. A cancelled invoice still shows the actions and is marked `CANCELLED`. Every state shows loading, empty, error with retry and success.

## 4. Tasks (at most 8)
- [ ] 1. Service and models: fetch the PDF (`GET invoices/{id}/pdf`) as a blob and open it.
- [ ] 2. Invoice view: replace the per-line GST with the rate-wise summary and add the amount in words (R32).
- [ ] 3. Print and Download PDF actions; the layout follows the invoice's chosen layout (R33).
- [ ] 4. Cancelled invoice keeps the actions and shows the `CANCELLED` mark.
- [ ] 5. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R32`, `R33`, `AC24` | 2, 3 | The invoice screen shows the same rate-wise GST summary, totals and amount in words as the PDF; Print and Download PDF produce the same document |

## 6. Not verified
To be filled when the plan is closed (the owner's look in the browser, and that screen and PDF agree).

## 7. Risks and notes
Written without a browser.
