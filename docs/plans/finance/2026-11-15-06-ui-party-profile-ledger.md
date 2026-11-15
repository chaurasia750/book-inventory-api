# Plan finance#06: UI school and vendor profiles with the ledger inside

- **File:** `docs/plans/finance/2026-11-15-06-ui-party-profile-ledger.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - section 6, R9, R11, R17 (the screen side); AC5, AC6, AC9; platform U14
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `finance#05`, `finance#04` (its statement panel is reused)
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: profile panel with tabs, balance column, the same for vendors)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/item-price-menu`, with the owner's other menu changes)

## 1. In one minute
After this plan there is no Ledger menu and no ledger page. Opening a school shows its profile beside the list: a header (code, name, status, balance for the Owner) and tabs Profile, Classes and Ledger; a vendor the same with Profile and Ledger. The Owner sees a Balance column in the school and vendor lists and a "with a balance only" filter (largest first, with the total). Open `/<company>/schools`, click a school, choose the Ledger tab.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/schools` and `pages/vendors` detail panels become profiles with tabs; the lists get the Balance column and the filter; `pages/ledger`: the statement becomes a tab component, the outstanding page and its routes are removed; the menu entries "School ledger" and "Vendor ledger" are removed), `libs/api-services` (`LedgerService.balances`), the three `en.json` catalogues.
Leaves out: later document tabs (they come with their modules), PDF and Excel.

## 3. Approach
A small tab bar in the existing detail panels (the classes editor already lives in the school panel: it becomes the Classes tab). The Ledger tab embeds the statement component of finance#04 without its own header and X. The lists ask `ledger/balances/...` for the ids of the shown page (Owner only); the filter "with a balance only" switches the list to the outstanding endpoint (R11), which is already sorted largest first and carries the total.

## 4. Tasks (at most 8)
- [x] 1. `LedgerService.balances` and the statement component usable as a tab.
- [x] 2. School profile: header with balance, tabs Profile, Classes, Ledger.
- [x] 3. Vendor profile: header with balance, tabs Profile, Ledger.
- [x] 4. School and vendor lists: Balance column and the "with a balance only" filter (Owner).
- [x] 5. Remove the ledger routes, the outstanding page and the two ledger menu entries; i18n.
- [ ] 6. Build and lint: 0 new findings; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R9`, `AC5` | 1, 2, 3 | A school's and a vendor's Ledger tab shows the statement with filters, opening row, running balance and totals |
| `R11`, `R17`, `AC6`, `AC9` | 4 | The lists show each balance; "with a balance only" shows the outstanding parties largest first with the total |
| U14 | 5 | The menu has no Ledger entries; Staff see no Balance column and no Ledger tab |

## 6. Not verified
Every row of section 5 is **NOT verified** in a browser (the assistant has none): the owner opens `/<company>/schools` and `/<company>/vendors`.

Done and observed: build `npx nx run-many -t build` -> "Successfully ran target build for 3 projects" with no warning; `npx nx run member:lint` -> only the known findings of `member-layout.ts`, `account-pages.ts`, `change-password.ts`. The endpoints were verified on the API side (finance#03, finance#05). The ledger page, its routes and the two ledger menu entries are removed; the statement is the Ledger tab; the balance in the profile header and the list column come from `ledger/balances`; "with a balance only" shows the outstanding list (largest first, total).

## 7. Risks and notes
Written without a browser; expect small visual fixes after the owner's first look.
