# Plan sales#02: UI school orders

- **File:** `docs/plans/sales/2026-11-15-02-ui-school-orders.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R2-R7, R14, R15 (the screen side); AC1-AC4
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `sales#01`, `school#06`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/sales-ui`)

## 1. In one minute
After this plan a school's profile has an **Orders** tab (list with status and totals) and "New order", which opens the full-width order editor: barcode and search entry, a price version per line, quantity, item discount (% or Rs), total discount, the GST toggle, live totals and the yellow stock warning; Save draft, Confirm, Delete draft, and the Owner's Close. Open a school, tab Orders.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/sales/orders`: list tab, editor page, routes), `libs/shared-models`, `libs/api-services`, i18n.
Leaves out: invoices (sales#04), PDF.

## 3. Approach
The editor shows the server's computed totals after each save (no money computed on the page; the preview calls the API). Barcode entry keeps the focus for fast scanning; the same item adds 1. Required fields carry a red `*`; the shared date picker is used (platform U15).

## 4. Tasks (at most 8)
- [x] 1. Models and `SchoolOrderService`.
- [x] 2. Orders tab in the school profile (list, status, totals, New order).
- [x] 3. Order editor page: header (school, date, session choice, GST), lines entry (barcode, search, version, qty, discount), stock warning.
- [x] 4. Totals panel (subtotal, discounts, GST split, round-off, grand total) from the API; Save, Confirm, Delete, Close.
- [x] 5. Routes, roles, i18n.
- [ ] 6. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`-`R7`, `AC1`-`AC4` | 2-5 | Create a draft, scan two books, change a discount and see the API totals; confirm; the Owner closes; Staff see no Close |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser; a 200-line order is the performance case to look at.
