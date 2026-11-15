# Plan sales#13: UI item discount follows the company setting

- **File:** `docs/plans/sales/2026-11-15-13-ui-item-discount-choice.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R36 (the screen side); AC28, AC29
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `sales#12`, `company#12`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** -
- **Created:** 2026-11-15

## 1. In one minute
After this plan the order editor, the order and the invoice screen show the discount column only when the company uses it (or the document already has a line discount); otherwise only the total discount is there. Open an order and an invoice with the setting on and off.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/sales`: the order editor, the invoice view, the print), the company setting read, i18n.
Leaves out: the company profile switch (company#13).

## 3. Approach
The screens hide the line discount field and the column when the response says the document does not show it; the totals keep the total discount. A refusal `sales.item_discount_disabled` shows its catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. Read the "shows the discount column" fact and the company setting where the editor needs it.
- [ ] 2. Order editor: hide the line discount field and column when off; keep total discount.
- [ ] 3. Invoice view and print: hide the column when off.
- [ ] 4. i18n; build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R36`, `AC28`, `AC29` | 1-3 | Setting off: no Discount column in the editor, the invoice screen or the print; only the total discount; setting on: the column is there; a document with a line discount still shows it after switching off |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a full browser session.
