# Plan platform#10: UI tables as cards on a phone

- **File:** `docs/plans/platform/2026-11-16-10-ui-table-cards.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U18; PAC21
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** none
- **UI plan:** this is itself a UI plan (the feature has no backend)
- **Approved by / on:** -
- **Created:** 2026-11-16

## 1. In one minute
After this plan a phone (under 768 px) shows every table as cards, one card per row, with labelled lines and no sideways scroll; a desktop screen looks as before. Open the schools, items, orders or payments list at a narrow width.

## 2. Touches / leaves out
Touches: the shared styles of `@org/ui` (one rule for a table that asks for it, with the labels taken from the header text), then each page that has a table (28 places in `apps/member`, `libs/ui` table and data grid, the audit and login-history screens of `admin`), i18n none.
Leaves out: the animation (platform#11); the PDF documents; the public site.

## 3. Approach
One shared mechanism: a table carries a class (and a small directive copies each header text into its cells as the line label), a media query under 768 px lays the rows out as cards (first cell the title, the others labelled lines, the action cell last, empty cells hidden). Pages are then switched on one group at a time, checked at 375, 768 and 1280 px.

## 4. Tasks (at most 8)
- [ ] 1. The shared rule and the directive in `@org/ui`, with one sample screen.
- [ ] 2. The list pages: schools, items, vendors, staff, orders, invoices.
- [ ] 3. Stock, prices (list, versions, bulk), payments, ledger, audit log, login history.
- [ ] 4. The tables inside side panels and dialogs: receipt, adjust, receive payment, bulk picker, class list editor, import dialogs.
- [ ] 5. The tables with inputs: order lines, make invoice, invoice lines.
- [ ] 6. Verify PAC21 on each screen at 375, 768 and 1280 px; build and lint.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U18`, `PAC21` | 1-6 | at 375 px each listed screen shows cards with labelled lines and `scrollWidth` equals `clientWidth` for the page and every table box; at 768 and 1280 px the tables look as before; a clickable list row is still one clickable card |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The biggest UI change so far (28 tables): done group by group, each group checked before the next. Seen through headless Chrome at three widths only.
