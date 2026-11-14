# Plan iam#04: UI - the Owner's Staff screens

- **File:** `docs/plans/iam/2026-11-14-04-ui-owner-staff-screens.md`
- **Kind:** UI
- **Spec:** [../../specs/iam.md](../../specs/iam.md) - rules covered (screens): R65, R66, R67; AC36, AC37, AC38
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `iam#03`, `iam#02`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
In the company workspace (`member` app) the Owner gets a "Staff" menu: a list of the company's Staff, a form to add one (the user id and temporary password are shown once), and a detail page to deactivate, reactivate, unlock, log out and reset the password. Staff and Owner-less accounts never see this menu. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `apps/member` (routes, menu item, three pages guarded for `OWNER`), `libs/api-services` (`StaffService`), `libs/shared-models`, the i18n catalogue (new `staff` group). Endpoints: `POST staff`, `GET staff`, `GET staff/{id}`, `GET staff/{id}/sessions`, the four status actions, `POST staff/{id}/temporary-password`. Leaves out: the Super Admin's own account screens (they already exist in `admin`), business screens.

## 3. Approach
Pages follow the existing user-management pages in `apps/admin` (list, filters, paging, detail, dialogs). Screen states for each page: loading, empty ("No staff yet"), error with retry, success. The credentials panel shows the user id and password once with a copy button and a warning that it cannot be shown again; leaving the panel clears it from memory. Deactivate asks for a reason.

## 4. Tasks (at most 8)
- [ ] 1. Models and `StaffService` for the endpoints above.
- [ ] 2. Staff list: search, paging, status chips, empty and error states (R66).
- [ ] 3. Add Staff form: full name and short name with the API's validation, the credentials panel (R65).
- [ ] 4. Staff detail: profile, status, sessions, actions with reason dialog for deactivate (R66).
- [ ] 5. Reset password action showing the new temporary password once (R67).
- [ ] 6. Routes, `OWNER` guard, menu item, i18n keys (no hard-coded text).
- [ ] 7. `npx nx run-many -t build` and `-t lint`: 0 new errors compared with the base branch.
- [ ] 8. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| `R66`, `AC37` | 2, 4, 6 | Owner of A sees only A's Staff; deactivate without a reason is blocked, with a reason the status changes; Staff and Super Admin do not see the menu and a direct URL gives the unauthorized page |
| `R65`, `AC36` | 3 | Add "Ramesh" -> panel shows `<ownerid>.ramesh` and the password once, gone after leaving; the same short name again shows the catalogue message |
| `R67`, `AC38` | 5 | Reset shows a new password once; the old password no longer logs in |

## 6. Not verified
To be filled when the plan is closed (mobile widths and other browsers are not part of this plan).

## 7. Risks and notes
Lint findings in files this plan does not touch are not part of it.
