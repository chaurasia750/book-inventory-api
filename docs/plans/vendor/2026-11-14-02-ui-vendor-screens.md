# Plan vendor#02: UI vendor screens

- **File:** `docs/plans/vendor/2026-11-14-02-ui-vendor-screens.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R2-R8, R10 (the screen side); AC1, AC4-AC8, AC10
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** `vendor#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-14 (chat: "approved hai kro implement")
- **Created:** 2026-11-14
- **Base branch:** `main` (UI repo; branch `feature/vendor-02` cut from it)

## 1. In one minute
After this plan the Owner has a "Vendors" menu in the company app: a list with search and filters; clicking a vendor opens it beside the list with an X to close it; adding and editing open in the same panel; activate and deactivate work. Open `/<company>/vendors` as an Owner.

## 2. Touches / leaves out
Touches: `libs/shared-models` (vendor model), `libs/api-services` (`VendorService`), `apps/member` (list, add, edit and detail components under `pages/vendors`, the `vendors` route with children `new`, `:id`, `:id/edit`, the "Vendors" menu item for the Owner), the three `en.json` catalogues (`vendor.*`, `errors.vendor.*`).
Leaves out: Staff access (none, R8), the Super Admin's screens in `admin`, Excel import.

## 3. Approach
It follows the list-and-detail pattern of platform U10 exactly as Schools does: `ListDetailComponent`, `injectPanelState`, detail component with `id` input and `closed` / `changed` outputs, add and edit as panel components, compact rows while a record is open. The bank block is one group of fields; the account number is shown masked in the list and in full only in the detail of the Owner. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [x] 1. Models (Indian state list for vendors, bank block) and `VendorService` for the endpoints.
- [x] 2. Vendor form (shared by add and edit): name, contact, mobile, email, GSTIN, state, bank block, opening balance; code shown read-only (R2, R5, R6, R11).
- [x] 3. List page with search, filters (status, state), paging, compact rows, states (R10).
- [x] 4. Detail panel with activate and deactivate (R7); add and edit panels.
- [x] 5. Routes, Owner guard, "Vendors" menu item under MANAGEMENT for the Owner only, i18n keys (R8).
- [x] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 7. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R5`, `R6`, `AC4`, `AC5` | 2 | Add a vendor with a wrong GSTIN, a partial bank block, a negative balance: the form and the API message show |
| `R10`, `AC10` | 3 | Search and filters narrow the list; paging works; compact rows while a record is open |
| `R7`, `AC7` | 4 | Deactivate and activate from the panel; the status pill changes and the list reloads |
| `R8`, `AC8` | 5 | An Owner sees the menu; a Staff account does not and a typed `/<slug>/vendors` answers the unauthorized page |

## 6. Not verified
- Built and linted: `nx run-many -t build` passes for all three apps; lint shows no finding in the new files (the findings of `member-layout.ts` and the account pages are the ones that already existed). Every fixed translation key the vendor screens use exists in the catalogue (script check). The routes `/abc-book-center/vendors` and `/abc-book-center/vendors/new` answer the member app on the single-port server.
- NOT verified (task 7, no browser tool): every row of section 5 on screen: the form messages, the list and filters, the panel, activate and deactivate, and that a Staff account does not see the menu and gets the unauthorized page on a typed address. The API behind it was verified with 37 checks (vendor#01). The plan stays `In progress` until the owner has looked at the screens (backlog B40).

## 7. Risks and notes
Same as the school screens: written without a browser; expect small visual fixes after the owner's first look.
