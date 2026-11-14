# Plan company#02: UI - Super Admin screens for companies

- **File:** `docs/plans/company/2026-11-14-02-ui-company-management-screens.md`
- **Kind:** UI
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered (screens): R2, R3, R4, R7, R8, R9, R11; AC1, AC2, AC6, AC7, AC9
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `company#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
In the `admin` console the Super Admin gets a "Companies" menu: a list of companies, a create form that shows the Owner's user id and temporary password once, and a detail page with the company's data, status history and accounts, where the company can be edited, deactivated and reactivated. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `apps/admin` (routes, menu item, three pages), `libs/api-services` (`CompanyService`, extended from iam#02 if it exists), `libs/shared-models` (company models), the i18n catalogue (new `company` group). Endpoints: `POST companies`, `GET companies`, `GET companies/{id}`, `PUT companies/{id}`, `POST companies/{id}/deactivate`, `POST companies/{id}/activate`. Leaves out: entering a company (company#04), logo, bank and invoice settings, the company workspace (iam#02).

## 3. Approach
Pages follow the existing user-management pages in `apps/admin` (list, filters, paging, detail, dialogs). The menu item and routes are Super Admin only (guard). Screen states for every page: loading, empty, error (with retry), success. The credentials panel shows the user id and temporary password once with a copy button and a warning that they cannot be shown again; leaving the panel clears them from memory.

## 4. Tasks (at most 8)
- [ ] 1. Models and `CompanyService` for the six endpoints.
- [ ] 2. Companies list page: search, status filter, paging, sort, empty and error states (R9).
- [ ] 3. Create page: form with validation as the API (company name 2-150 characters, Owner full name) and the one-time credentials panel (R3, R4).
- [ ] 4. Detail page: master data, status with reason, status history, accounts with a link to the existing user detail (R8, R9, R11).
- [ ] 5. Edit form and the deactivate (reason required) and reactivate dialogs (R2, R7).
- [ ] 6. Routes, Super Admin guard, menu item, i18n keys (no hard-coded text).
- [ ] 7. `npx nx run-many -t build` and `-t lint`: 0 new errors compared with the base branch.
- [ ] 8. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `R9` | 2 | List shows both companies; search by name and the status filter narrow it; paging works; empty and error states visible |
| `R3`, `R4`, `AC1`, `AC2` | 3 | Create "Children Book Center" -> the panel shows `childrenbookcenter` and the password once, gone after leaving; the same name again -> the catalogue message for `company.name_taken` |
| `R8`, `R9`, `R11` | 4 | Detail shows history rows and the Owner with a working link |
| `R2`, `R7`, `AC6`, `AC7`, `AC9` | 5 | Deactivate without a reason is blocked; with a reason the status changes and history grows; reactivate restores it |
| `R2` | 6 | An Owner opening the Companies URL sees the unauthorized page; the menu item is absent for them |

## 6. Not verified
To be filled when the plan is closed (mobile widths and other browsers are not part of this plan).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it.
