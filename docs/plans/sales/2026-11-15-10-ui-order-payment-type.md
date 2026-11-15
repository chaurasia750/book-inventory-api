# Plan sales#10: UI order payment type

- **File:** `docs/plans/sales/2026-11-15-10-ui-order-payment-type.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R31, R34 (the screen side); AC21, AC25
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `sales#09`, `school#08`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: Cash/Credit memo answered "recommended"; the owner asked to modify the existing invoice PDF spec and plans)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/invoice-pdf-ui`, shared by school#08, sales#10 and sales#08)

## 1. In one minute
After this plan the order editor has a Payment select (Cash / Credit) filled from the school, locked once something is invoiced, and the invoice screen shows its title (TAX INVOICE, CASH MEMO or CREDIT MEMO) and payment type. Open an order, then its invoice.

## 2. Touches / leaves out
Touches: `apps/member` order editor and invoice view, models, i18n.
Leaves out: the PDF template (sales#09).

## 3. Approach
One select beside GST in the order header, disabled with the same hint as the total discount once something is invoiced; the invoice header shows the same title as the PDF.

## 4. Tasks (at most 8)
- [x] 1. Models and the save request carry `paymentType`.
- [x] 2. The Payment select in the order editor (locked after an invoice).
- [x] 3. The invoice title and payment type on the invoice screen.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R34`, `R31`, `AC21`, `AC25` | 1-3 | New order for a Cash school shows Cash; change to Credit and invoice: the screen says CREDIT MEMO (GST off); after the invoice the select is disabled |

## 6. Not verified
Every row of section 5 is **NOT verified** in a browser: the owner starts a new order for a Cash school (Payment: Cash), changes it to Credit, makes the invoice (the screen says CREDIT MEMO when GST is off) and sees the select disabled after the invoice.

Done and observed: the order editor has a Payment select beside GST (filled from the school on a new order, saved with the order, disabled with the total discount and GST once something is invoiced); the invoice screen shows its title (TAX INVOICE / CASH MEMO / CREDIT MEMO, `GST Exempted` when GST is off) and the payment type under the number. `npx nx run-many -t build` -> "Successfully ran target build for 3 projects"; `npx nx run member:lint` -> only the three known findings (`member-layout.ts`, `account-pages.ts`, `change-password.ts`).

## 7. Risks and notes
Written without a browser.
