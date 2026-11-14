# Plan company#04: UI - company picker for the Super Admin

- **File:** `docs/plans/company/2026-11-14-04-ui-company-picker.md`
- **Kind:** UI
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered (screens): R13; AC13, AC14
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** `company#03`, `company#02`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -
- **Base branch:** `main` of `book-inventory-ui` (worked on `feature/company-accounts-ui`, UI repository; the plan files live in this API repository)

## 1. In one minute
At the top of the `admin` console a Super Admin sees a company picker: "No company" or the name of the company they entered. Choosing a company enters it (the name stays visible everywhere), "Leave company" goes back. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `apps/admin` layout header (picker component), `libs/api-services` (`CompanyService`: enter, leave, current), the i18n catalogue. Endpoints: `GET companies` (active only, for the list), `POST companies/{id}/enter`, `POST companies/leave`, `GET companies/current`. Leaves out: business screens that would use the entered company (later modules).

## 3. Approach
The picker lists active companies only (inactive ones are not offered, matching `409`). The chosen company is shown in the header until the user leaves or the session ends; the access token returned by enter replaces the one in memory. Screen states: loading list, empty list ("No active company"), error (retry, picker stays on its previous value), success.

## 4. Tasks (at most 8)
- [x] 1. `CompanyService` methods for enter, leave and current.
- [x] 2. Picker component in the `admin` header with its states.
- [x] 3. Keep the entered company visible in the header and clear it on leave and on logout.
- [x] 4. i18n keys (no hard-coded text); picker only for `SUPER_ADMIN`.
- [x] 5. `npx nx run-many -t build` and `-t lint`: 0 new errors compared with the base branch.
- [ ] 6. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `R13`, `AC13` | 1, 2, 3 | Choose "Children Book Center" -> header shows it; choose "ABC Book Center" -> header changes; Leave -> "No company" **Built:** `CompanyPickerComponent` in the admin header (projected through a new slot of `app-header` and `app-layout`): enter, switch, leave. **NOT seen:** the screen was not opened in a browser. |
| `R13`, `AC14` | 2, 4 | A deactivated company is not offered; an Owner never sees the picker **Built:** only active companies are listed and the picker is rendered for the Super Admin only. **NOT seen:** the screen was not opened in a browser. |

## 6. Not verified
**Checked (2026-11-14):** `npx nx run-many -t build` -> all three apps build; `npx nx run-many -t lint` -> no finding in any new or changed file (the 6 `Static imports of lazy-loaded libraries` errors in the layouts and account pages are identical on `main`); every endpoint this screen calls was exercised against the running API in the backend plans. **NOT seen:** the screen itself was not opened in a browser (no browser tool in this session).

Still open before this plan can be closed: task 6 (open every screen against the running API and record what was seen).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it.
