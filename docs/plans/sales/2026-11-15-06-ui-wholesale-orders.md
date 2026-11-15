# Plan sales#06: UI Orders menu and wholesale order editor

- **File:** `docs/plans/sales/2026-11-15-06-ui-wholesale-orders.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R19-R27 (the screen side); AC11-AC18
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `sales#05`, `planning#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/sales-06`, shared by planning#02 and sales#06 on the owner's instruction)

## 1. In one minute
After this plan BUSINESS has an **Orders** menu (Order list across schools, Invoices); New order asks for the school; the order editor has a class column, "Add class sets" (classes with student counts), a bulk picker (filters, tick many, one quantity and class), inline editing of quantity, class, version, discount and delete, also on confirmed orders within the limits. Open Orders > Order list.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/sales`: orders list page, invoices list page, school chooser for New order, editor changes, class set dialog, bulk item picker), routes and the Orders menu, models, service, i18n.
Leaves out: class-wise invoices, PDF.

## 3. Approach
The Orders menu entries reuse the list components of the school tabs without the school filter. The bulk picker is a dialog with filters and a selectable table. Locked cells (invoiced lines) are shown read-only with a hint.

## 4. Tasks (at most 8)
- [x] 1. Orders menu, order list and invoice list pages, school chooser.
- [x] 2. Editor: class column, inline edits, confirmed-order limits.
- [x] 3. Add class sets dialog.
- [x] 4. Bulk item picker (shared with planning#02).
- [x] 5. Easy new order (R27: school search only, then the three add ways side by side); invoice layout switch (by class / book list) on the invoice screen and the default in the school profile (R26).
- [x] 6. i18n, routes.
- [ ] 7. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`-`R25`, `AC11`-`AC16` | 1-4 | Orders menu lists all schools' orders; New order with a school; add Class 4 and 5 sets; bulk add; edit a confirmed order; refusals name the line; invoice switched between by class and book list; new order from a school only |

## 6. Not verified
Every row of section 5 is **NOT verified** in a browser (the assistant has none): the owner opens BUSINESS > Orders > Order list and Invoices, "New order" (school chooser), the order editor (the three ways on an empty order, class column, "Add class sets", "Bulk add", editing a confirmed order with locked cells, the refusal text naming the line), an invoice with the Book list / By class switch, and the invoice layout default on the school profile.

Done and observed: one commit on `feature/sales-06` of `book-inventory-ui`: models and `SalesService` (class on lines, `lineId`, class sets, bulk lines, invoice layout); `pages/sales`: `documents-page` (routes `/orders`, `/invoices`, menu BUSINESS > Orders), `school-chooser`, `school-documents-tab` now also lists every school's documents with search and status filter, `class-set-dialog` (classes with the planning estimate prefilled), `bulk-item-picker` (filters, tick, one quantity and class), `order-editor` rewritten (R27 three ways side by side, class column, confirmed-order edits within R20 with invoiced cells locked, "Save changes", notices naming what was left out, `{{subject}}` in the refusal texts), `invoice-view` (layout switch, class groups with subtotals), `invoice-layout-setting` on the school profile. `npx nx run-many -t build` -> "Successfully ran target build for 3 projects"; `npx nx run member:lint` -> only the three known findings.

## 7. Risks and notes
Written without a browser.
