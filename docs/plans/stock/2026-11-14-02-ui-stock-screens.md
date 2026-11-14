# Plan stock#02: UI stock screens

- **File:** `docs/plans/stock/2026-11-14-02-ui-stock-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R7-R10, R16 (the screen side); AC1-AC3, AC6
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `stock#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner and Staff have a "Stock" menu in the company app: the stock list with Main, Damaged and Sample quantities and a warning mark at zero or below; clicking a book opens its stock beside the list with an X to close, showing its history; the Owner enters the opening stock and adjustments there. Open `/<company>/stock`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (stock model), `libs/api-services` (`StockService`), `apps/member` (list and item stock panel with history, opening and adjustment forms under `pages/stock`, the `stock` route with child `:id`, the "Stock" menu item for the Owner and Staff in its own group), the three `en.json` catalogues (`stock.*`, `errors.stock.*`).
Leaves out: document screens (Purchase, Invoice, Counter, Return, Sample), the Excel upload screen (stock#04), the Super Admin's screens.

## 3. Approach
It follows the list-and-detail pattern of platform U10 as the other screens do. The list shows a red mark on a quantity at zero or below and a "zero or below only" filter. The panel shows the three quantities, the history (date, kind, document number, in, out, quantity after, who) and, for the Owner, two small inline forms: Opening (stock type, quantity, date) and Adjustment (stock type, counted quantity, reason), with the required `*` markers and the API messages. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models and `StockService` for the endpoints.
- [ ] 2. Stock list page: search, filters (category, zero or below), paging, warning marks, compact rows, states (R9, R10).
- [ ] 3. Item stock panel: three quantities and the history (R10).
- [ ] 4. Opening and adjustment forms for the Owner (R7, R8).
- [ ] 5. Routes, role guards (Owner writes, Staff read), the "Stock" menu item, i18n keys.
- [ ] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 7. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R7`, `AC1` | 4 | Enter an opening; a second opening for the same type shows the API message |
| `R8`, `AC2` | 4 | Enter a counted quantity with a reason; the history shows the adjustment and the new quantity |
| `R10`, `AC3` | 3 | The history shows each entry with the quantity after it |
| `R9`, `R10`, `AC6` | 2 | Quantities at zero or below carry the warning mark; the filter shows only them; search and paging work |
| `R10` | 5 | Staff see the list and history but no forms |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
