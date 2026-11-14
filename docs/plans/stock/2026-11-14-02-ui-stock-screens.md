# Plan stock#02: UI stock screens

- **File:** `docs/plans/stock/2026-11-14-02-ui-stock-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R7-R10, R16 (the screen side); AC1-AC3, AC6
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `stock#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/stock-ui` for stock#02 and stock#04, the owner's instruction)

## 1. In one minute
After this plan the Owner and Staff have a "Stock" menu in the company app: the stock list with Main, Damaged and Sample quantities and a warning mark at zero or below; clicking a book opens its stock beside the list with an X to close, showing its history; the Owner enters the opening stock and adjustments there. Open `/<company>/stock`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (stock model), `libs/api-services` (`StockService`), `apps/member` (list and item stock panel with history, opening and adjustment forms under `pages/stock`, the `stock` route with child `:id`, the "Stock" menu item for the Owner and Staff in its own group), the three `en.json` catalogues (`stock.*`, `errors.stock.*`).
Leaves out: document screens (Purchase, Invoice, Counter, Return, Sample), the Excel upload screen (stock#04), the Super Admin's screens.

## 3. Approach
It follows the list-and-detail pattern of platform U10 as the other screens do. The list shows a red mark on a quantity at zero or below and a "zero or below only" filter. The panel shows the three quantities, the history (date, kind, document number, in, out, quantity after, who) and, for the Owner, two small inline forms: Opening (stock type, quantity, date) and Adjustment (stock type, counted quantity, reason), with the required `*` markers and the API messages. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [x] 1. Models and `StockService` for the endpoints.
- [x] 2. Stock list page: search, filters (category, zero or below), paging, warning marks, compact rows, states (R9, R10).
- [x] 3. Item stock panel: three quantities and the history (R10).
- [x] 4. Opening and adjustment forms for the Owner (R7, R8).
- [x] 5. Routes, role guards (Owner writes, Staff read), the "Stock" menu item, i18n keys.
- [x] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
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
Every row of section 5 is **NOT verified**: the assistant has no browser; the owner looks at the screens (`/<company>/stock`) against the running API.

Done and observed: commit `d56ab2e` on `feature/stock-ui` (stock model and `StockService`; `pages/stock`: `stock-list`, `item-stock`, `stock-opening-form`, `stock-adjustment-form`; route `stock` with child `:id`; menu "Stock" in its own group for Owner and Staff; `stock.*` and `errors.stock.*` in the three catalogues); `npx nx run-many -t build` -> "Successfully ran target build for 3 projects"; `npx nx run-many -t lint` -> no finding in any stock file (the findings shown are the known ones of `member-layout.ts`, `account-pages.ts`, `change-password.ts` and the libraries, unchanged). The endpoints the screens call were verified on the API side (stock#01 / stock#03 checks).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
