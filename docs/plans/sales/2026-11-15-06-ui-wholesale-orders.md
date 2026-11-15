# Plan sales#06: UI Orders menu and wholesale order editor

- **File:** `docs/plans/sales/2026-11-15-06-ui-wholesale-orders.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R19-R25 (the screen side); AC11-AC16
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `sales#05`, `planning#02`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan BUSINESS has an **Orders** menu (Order list across schools, Invoices); New order asks for the school; the order editor has a class column, "Add class sets" (classes with student counts), a bulk picker (filters, tick many, one quantity and class), inline editing of quantity, class, version, discount and delete, also on confirmed orders within the limits. Open Orders > Order list.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/sales`: orders list page, invoices list page, school chooser for New order, editor changes, class set dialog, bulk item picker), routes and the Orders menu, models, service, i18n.
Leaves out: class-wise invoices, PDF.

## 3. Approach
The Orders menu entries reuse the list components of the school tabs without the school filter. The bulk picker is a dialog with filters and a selectable table. Locked cells (invoiced lines) are shown read-only with a hint.

## 4. Tasks (at most 8)
- [ ] 1. Orders menu, order list and invoice list pages, school chooser.
- [ ] 2. Editor: class column, inline edits, confirmed-order limits.
- [ ] 3. Add class sets dialog.
- [ ] 4. Bulk item picker (shared with planning#02).
- [ ] 5. i18n, routes.
- [ ] 6. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R19`-`R25`, `AC11`-`AC16` | 1-4 | Orders menu lists all schools' orders; New order with a school; add Class 4 and 5 sets; bulk add; edit a confirmed order; refusals name the line |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
