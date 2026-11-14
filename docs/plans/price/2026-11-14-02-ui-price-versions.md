# Plan price#02: UI price screens (list, versions, copy, history)

- **File:** `docs/plans/price/2026-11-14-02-ui-price-versions.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R2-R9, R15 (the screen side); AC1-AC3, AC5, AC7, AC8, AC12
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** `price#01`
- **Base branch:** `main` (UI repo; the work sits on `feature/price-02`, cut from it)
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner and Staff have a "Prices" menu in the company app: the price list with each item's default version; clicking an item opens its versions beside the list with an X to close; the Owner adds and edits versions in the same panel, copies a version to a new year and reads the history of a version; Staff see the same read-only without purchase prices. Open `/<company>/prices`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (price model), `libs/api-services` (`PriceService`, a small history call on the existing audit service), `apps/member` (list, versions panel, version form, copy dialog and history components under `pages/prices`, the `prices` route with child `:itemId`, the "Prices" menu item for the Owner and Staff in its own group), the three `en.json` catalogues (`price.*`, `errors.price.*`).
Leaves out: the bulk change screens (price#04), Excel upload, the Super Admin's screens in `admin`.

## 3. Approach
It follows the list-and-detail pattern of platform U10: `ListDetailComponent`, `injectPanelState` (the panel shows an item's versions; adding and editing a version are inline panels inside it), compact rows while a panel is open. The default version is marked with a pill. The version form uses the shared required marker (`*`) on name, date and the three amounts. The history of a version is read through the existing audit endpoint (`GET audit?entityType=PriceVersion&entityId=...`) and shown as a small timeline; Staff do not see the history button. The Copy dialog takes the source name, the new name, the date and an optional vendor, category or class. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [x] 1. Models and `PriceService` for the endpoints; the history call.
- [x] 2. Price list page: search, filters (category), paging, compact rows, states (R9).
- [x] 3. Versions panel: versions newest first, the default marked, Staff without purchase price (R4, R5).
- [x] 4. Add and edit version form with the required markers and the API messages (R2, R3).
- [x] 5. Copy dialog and the history timeline (R8, R15).
- [x] 6. Routes, role guards (Owner writes, Staff read), the "Prices" menu item, i18n keys.
- [ ] 7. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base; verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R3`, `AC1`, `AC3` | 4 | Add a version with a negative amount, three decimals, a name already used: the form or the API message shows; an edit keeps the name unique |
| `R4`, `AC2` | 3 | The default pill is on the newest applicable version |
| `R5`, `AC5` | 3, 6 | The Owner sees purchase prices and the edit buttons; Staff see neither purchase prices nor buttons |
| `R8`, `AC8` | 5 | The Copy dialog reports how many items were copied and skipped |
| `R9`, `AC7` | 2 | Search and filters narrow the list; paging works; compact rows while a panel is open |
| `R15`, `AC12` | 5 | The history of a version shows who, when and the old and new values |

## 6. Not verified

Written and checked on 2026-11-14 on branch `feature/price-02` (UI repo), base `main`.

**Ran**
- `npx nx run-many -t build --skip-nx-cache`: 3 projects succeed.
- `npx nx lint member --skip-nx-cache`: no new findings; only the three pre-existing "Static imports of lazy-loaded libraries are forbidden" errors (`member-layout.ts`, `account-pages.ts`, `change-password.ts`).
- i18n script: all 51 `price.*` keys and all 7 `errors.price.*` keys present in the three `en.json` catalogues.
- API smoke checks against the running API on the local dev database (`book_inventory`), Owner `abcbookcenter`:
  - `GET prices`: 30 items, paged (pageSize 5), each row with the default version's name, date and amounts; page 2 works; `pageSize=500` capped to 200.
  - `GET prices?category=BOOK&search=a`: 11 results; search by barcode `8901234567890` and by code `ITM-0020` finds the item.
  - Add version: negative amount, three decimals and a one-character name answer `400 price.invalid_amount` / `price.invalid_name`; a valid add answers `201`; the same name again (with spaces or other case) answers `409 price.name_taken`; a future-dated version is accepted and is not the default.
  - Versions newest first: `UI-CHECK-27@2027-04-01 default=False`, `UI-CHECK-26@2026-04-01 default=True`.
  - Edit answers `204`; the audit trail shows `UPDATED` and `CREATED` with the actor `abcbookcenter` and the snake_case old/new values (R15).
  - Copy `UI-CHECK-26` -> `UI-COPY-27` for BOOK: `{itemsSelected: 14, copied: 1, skippedNoSourceVersion: 13, skippedAlreadyHasTarget: 0}`; repeating it reports `copied 0, alreadyHas 1`; the copy became the default on the equal date (R4).

**NOT verified**
- No browser tool in the session: none of the screens (list, versions panel, add/edit form, copy dialog, history timeline, Staff view without purchase prices, compact rows, menu and guards) was opened. Every row of section 5 is therefore NOT verified on screen; only the API behaviour behind them was. The plan stays `In progress` until the Owner looks at `/<company>/prices` (same gap as B17, B40, B42).

## 7. Risks and notes
Same as the other screens: written without a browser; expect small visual fixes after the owner's first look.
