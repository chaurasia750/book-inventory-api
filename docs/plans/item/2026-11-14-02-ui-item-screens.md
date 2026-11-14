# Plan item#02: UI item screens

- **File:** `docs/plans/item/2026-11-14-02-ui-item-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R2-R9, R12 (the screen side); AC1, AC4-AC8, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `item#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: "all approved hai kro implement")
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner and Staff have an "Items" menu in the company app: a list with search and filters; clicking an item opens it beside the list with an X to close it; the Owner adds and edits items and edits their barcodes in the same panel and activates or deactivates them; Staff see the list and the detail read-only. Open `/<company>/items`.

## 2. Touches / leaves out
Touches: `libs/shared-models` (item model, categories, GST rates), `libs/api-services` (`ItemService`), `apps/member` (list, add, edit and detail components under `pages/items`, the `items` route with children `new`, `:id`, `:id/edit`, the "Items" menu item for the Owner and Staff), the three `en.json` catalogues (`item.*`, `errors.item.*`).
Leaves out: prices, stock, the scan flow, the Super Admin's screens in `admin`, Excel import.

## 3. Approach
It follows the list-and-detail pattern of platform U10 exactly as Schools and Vendors do: `ListDetailComponent`, `injectPanelState`, a detail component with `id` input and `closed` / `changed` / `editRequested` outputs, add and edit as panel components, compact rows while a record is open. The barcode list is an editor inside the detail (add, remove, save, with the duplicate check done by the API's `409`) like the school's class list. The vendor is chosen from the Owner's active vendors (`GET vendors`, a select with search); Staff cannot read vendors, so for them the detail shows the vendor name the item response carries. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models (categories, GST rates list, item and barcode shapes) and `ItemService` for the endpoints.
- [ ] 2. Item form (shared by add and edit): name, category, vendor, class, subject, HSN, GST rate, stock managed; the code shown read-only (R2-R4, R7, R10).
- [ ] 3. List page with search, filters (category, status, stock managed), paging, compact rows, states (R12).
- [ ] 4. Detail panel with the barcode editor and activate and deactivate (R6, R8); add and edit panels.
- [ ] 5. Routes, role guards (Owner writes, Staff read), the "Items" menu item for both, i18n keys (R9).
- [ ] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 7. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R3`, `R4`, `R7`, `AC4` | 2 | Adding a Pen shows stock managed off by default, a Book on; a name of one character and a GST value outside the list are refused by the form or the API message |
| `R5`, `AC5` | 2 | The vendor select offers active vendors only; the detail shows the vendor's name |
| `R6`, `AC6` | 4 | Adding the same barcode to a second item shows the API message; removing a barcode and saving frees it |
| `R12`, `AC10` | 3 | Search and filters narrow the list; paging works; compact rows while a record is open |
| `R8`, `AC7` | 4 | Deactivate and activate from the panel; the status pill changes and the list reloads |
| `R9`, `AC8` | 5 | The Owner sees Add, Edit and status buttons; Staff see none of them and no inactive items |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
Same as the school and vendor screens: written without a browser; expect small visual fixes after the owner's first look.
