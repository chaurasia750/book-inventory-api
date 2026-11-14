# Plan finance#04: UI ledger screens

- **File:** `docs/plans/finance/2026-11-14-04-ui-ledger-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R9-R11, R13, R16 (the screen side); AC5-AC7
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `finance#03`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner has a "Ledger" menu in its own group: "Outstanding" with a Schools tab and a Vendors tab (balance, largest first, total, search); clicking a row opens that party's statement beside the list with an X to close (platform U10): the filters (financial year, session for schools, from-to dates, all), the opening row, every entry with its running balance and the totals. A school's and a vendor's own panel get a "Ledger" link to the same statement. Open `/<company>/ledger`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (ledger models), `libs/api-services` (`LedgerService`), `apps/member` (`pages/ledger`: outstanding list with tabs, statement panel; the `ledger` route with children; the "Ledger" menu for the Owner; a "Ledger" link in the school and vendor panels), the three `en.json` catalogues (`ledger.*`, `errors.finance.*`).
Leaves out: PDF and Excel buttons (Q7), the "Pending" cheque mark, opening a document from a row, Staff screens (none, R13).

## 3. Approach
It follows the price and stock list-and-detail screens. Amounts are shown with two decimals and a Dr/Cr label by the sign of R7; the opening row and the totals row are set apart; a negative balance (advance) is shown as such. The filters change the request only; nothing is computed on the page. Each state shows loading, empty, error with retry and success; all texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models and `LedgerService` for the four endpoints.
- [ ] 2. Outstanding page: Schools and Vendors tabs, search, paging, the total, inactive mark, states (R11, R16).
- [ ] 3. Statement panel: filters, opening row, rows with running balance, totals, the "narrow the filter" message (R9, R10).
- [ ] 4. "Ledger" link in the school and vendor panels opening the statement.
- [ ] 5. Routes, the Owner's role guard, the "Ledger" menu item, i18n keys.
- [ ] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base; verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R11`, `R16`, `AC6` | 2 | The outstanding tabs show the balances largest first with the total; search narrows them |
| `R9`, `R10`, `AC5` | 3 | A school's statement shows the opening row, the running balance and totals; the year, session and date filters change the opening row |
| `R13`, `AC7` | 5 | Staff do not see the menu and the route answers access denied |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
