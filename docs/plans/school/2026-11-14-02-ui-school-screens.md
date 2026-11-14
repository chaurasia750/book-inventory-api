# Plan school#02: UI - school screens

- **File:** `docs/plans/school/2026-11-14-02-ui-school-screens.md`
- **Kind:** UI
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered (screens): R2-R9, R11, R15; AC1, AC4-AC7, AC9
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
In the company workspace (`member` app) the Owner gets a "Schools" menu: a list with search and filters, an add and edit form with the class list editor, and activate and deactivate. Staff get the same list read-only (active schools only). Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `apps/member` (routes, menu item, three pages), `libs/api-services` (`SchoolService`), `libs/shared-models` (school models and the state list), the i18n catalogue (new `school` group). Endpoints: `POST schools`, `GET schools`, `GET schools/{id}`, `PUT schools/{id}`, `PUT schools/{id}/classes`, `POST schools/{id}/deactivate` and `/activate`. Leaves out: the Super Admin's school screens in the `admin` app (Q11), Excel import (Q9), session derivation.

## 3. Approach
Pages follow the Staff and company pages already built (list with search, filters and paging; detail with dialogs). The Owner's menu item and routes are guarded for `OWNER`; Staff get a read-only list and detail (no add, edit or status buttons) guarded for `OWNER` and `STAFF`. The class list editor is an ordered list with add, rename, move up and down, and remove; duplicates are shown before saving. Screen states for every page: loading, empty, error (with retry), success. All texts come from the catalogue.

## 4. Tasks (at most 8)
- [ ] 1. Models (the state list, which holds only Bihar, D6) and `SchoolService` for the endpoints.
- [ ] 2. School list page: search, filters (status, sale type, session type, state), paging, states (R11).
- [ ] 3. Add and edit form: name, contact, mobile, email, GSTIN, state (Bihar), session type, sale type, GST default, opening balance; the code is shown read-only after creation (R2-R5, R7, R15).
- [ ] 4. Class list editor (R6).
- [ ] 5. Detail page with activate and deactivate (R8); read-only view for Staff (R9).
- [ ] 6. Routes, role guards, menu items, i18n keys (no hard-coded text).
- [ ] 7. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 8. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `R11`, `AC9` | 2 | Search and each filter narrow the list; paging works; empty and error states visible |
| `R2`-`R5`, `R7`, `AC1`, `AC4` | 3 | Add a school: the form has no code field, the saved school shows its generated code (`SCH-0001`); the form shows the API's validation text for a bad name and a negative balance; a saved school appears in the list |
| `R6`, `AC5` | 4 | Reordering and renaming classes saves in that order; a duplicate is refused before saving |
| `R8`, `R9`, `AC6`, `AC7` | 5, 6 | Deactivate and activate change the status badge; a Staff login sees the list read-only and a direct add URL gives the unauthorized page |

## 6. Not verified
To be filled when the plan is closed (mobile widths and other browsers are not part of this plan).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it. The state list lives in the UI for the form and is validated again by the API (D6).
