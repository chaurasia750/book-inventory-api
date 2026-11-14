# Plan price#02: UI price screens (list, versions, copy, history)

- **File:** `docs/plans/price/2026-11-14-02-ui-price-versions.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R2-R9, R15 (the screen side); AC1-AC3, AC5, AC7, AC8, AC12
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `price#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** pending
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner and Staff have a "Prices" menu in the company app: the price list with each item's default version; clicking an item opens its versions beside the list with an X to close; the Owner adds and edits versions in the same panel, copies a version to a new year and reads the history of a version; Staff see the same read-only without purchase prices. Open `/<company>/prices`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (price model), `libs/api-services` (`PriceService`, a small history call on the existing audit service), `apps/member` (list, versions panel, version form, copy dialog and history components under `pages/prices`, the `prices` route with child `:itemId`, the "Prices" menu item for the Owner and Staff in its own group), the three `en.json` catalogues (`price.*`, `errors.price.*`).
Leaves out: the bulk change screens (price#04), Excel upload, the Super Admin's screens in `admin`.

## 3. Approach
It follows the list-and-detail pattern of platform U10: `ListDetailComponent`, `injectPanelState` (the panel shows an item's versions; adding and editing a version are inline panels inside it), compact rows while a panel is open. The default version is marked with a pill. The version form uses the shared required marker (`*`) on name, date and the three amounts. The history of a version is read through the existing audit endpoint (`GET audit?entityType=PriceVersion&entityId=...`) and shown as a small timeline; Staff do not see the history button. The Copy dialog takes the source name, the new name, the date and an optional vendor, category or class. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models and `PriceService` for the endpoints; the history call.
- [ ] 2. Price list page: search, filters (category), paging, compact rows, states (R9).
- [ ] 3. Versions panel: versions newest first, the default marked, Staff without purchase price (R4, R5).
- [ ] 4. Add and edit version form with the required markers and the API messages (R2, R3).
- [ ] 5. Copy dialog and the history timeline (R8, R15).
- [ ] 6. Routes, role guards (Owner writes, Staff read), the "Prices" menu item, i18n keys.
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
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Same as the other screens: written without a browser; expect small visual fixes after the owner's first look.
