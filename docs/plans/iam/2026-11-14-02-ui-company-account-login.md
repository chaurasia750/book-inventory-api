# Plan iam#02: UI - company accounts log in and see their company

- **File:** `docs/plans/iam/2026-11-14-02-ui-company-account-login.md`
- **Kind:** UI
- **Spec:** [../../specs/iam.md](../../specs/iam.md) R60, R63, R64, AC33, AC35; [../../specs/company.md](../../specs/company.md) R10, Q13 - rules covered: iam R63 (message), R64 (user id field), R60 (routing), company R10 and AC4 (UI part)
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `iam#01`, `company#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
A company's Owner or Staff opens the login page, types their user id and password, and lands in the company workspace (the `member` app) with the company's name shown at the top. A Super Admin still lands in the `admin` console. If the company is inactive, the login screen shows the catalogue message instead of signing in. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `libs/auth` (role to app routing for `OWNER` and `STAFF`), `libs/guards` (which roles each app accepts), `libs/layout` (company name in the header), the login page in `apps/public-site` (the identifier is already one field; its label says "User ID"), the `apps/member` layout. Endpoints used: `POST auth/login`, `GET auth/me`, `GET companies/current`. Leaves out: any business screen, renaming the `member` app, the Owner's Staff screens (iam#04) and the Super Admin screens (company#02, company#04).

## 3. Approach
The existing login redirect decides the app from the role; `OWNER` and `STAFF` are added and sent to the `member` app (company spec Q13). The company name comes from `companies/current` once after login and is kept in memory only. Screen states. Login: loading, wrong credentials (generic text), company inactive (catalogue text), success. Header: loading (placeholder), error (app name only), success (company name).

## 4. Tasks (at most 8)
- [ ] 1. Add `OWNER` and `STAFF` to the shared role model and the role-to-app routing; the `member` app guards accept them, the `admin` app does not.
- [ ] 2. Show the catalogue message for `iam.auth.company_inactive` on the login page (i18n key, no hard-coded text); the field label reads "User ID".
- [ ] 3. `CompanyService` in `libs/api-services` for `GET companies/current`.
- [ ] 4. The `member` layout header shows the company name (states above).
- [ ] 5. `npx nx run-many -t build` and `-t lint`: 0 new errors compared with the base branch.
- [ ] 6. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| iam `R63`, `AC33` (message) | 2 | Inactive company, correct password -> the login page shows the catalogue text; wrong password -> the generic text |
| iam `R64`, `AC35` (field) | 2 | The login form asks for a user id; an email typed there gives the generic error |
| iam `R60` (routing) | 1 | Owner and Staff login -> `member` app URL; Super Admin login -> `admin` app URL; Owner opening the `admin` URL -> unauthorized page |
| company `R10`, `AC4` (UI part) | 3, 4 | Owner login -> the top bar shows the company's name; with `companies/current` failing -> the app name only |

## 6. Not verified
To be filled when the plan is closed (cross-browser and mobile widths are not part of this plan).

## 7. Risks and notes
The role strings of the UI must match the API's `OWNER` and `STAFF`. Lint findings in files this plan does not touch are not part of it.
