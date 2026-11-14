# Plan iam#02: UI - company accounts log in and see their company

- **File:** `docs/plans/iam/2026-11-14-02-ui-company-account-login.md`
- **Kind:** UI
- **Spec:** [../../specs/iam.md](../../specs/iam.md) R60, R63, R64, AC33, AC35; [../../specs/company.md](../../specs/company.md) R10, Q13 - rules covered: iam R63 (message), R64 (user id field), R60 (routing), company R10 and AC4 (UI part)
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** `iam#01`, `company#01`
- **UI plan:** - (this is the UI plan)
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -
- **Base branch:** `main` of `book-inventory-ui` (worked on `feature/company-accounts-ui`, UI repository; the plan files live in this API repository)

## 1. In one minute
A company's Owner or Staff opens the login page, types their user id and password, and lands in the company workspace (the `member` app) with the company's name shown at the top. A Super Admin still lands in the `admin` console. If the company is inactive, the login screen shows the catalogue message instead of signing in. Repository: `book-inventory-ui` (build: `npx nx run-many -t build`, lint: `npx nx run-many -t lint`).

## 2. Touches / leaves out
Touches: `libs/auth` (role to app routing for `OWNER` and `STAFF`), `libs/guards` (which roles each app accepts), `libs/layout` (company name in the header), the login page in `apps/public-site` (the identifier is already one field; its label says "User ID"), the `apps/member` layout. Endpoints used: `POST auth/login`, `GET auth/me`, `GET companies/current`. Leaves out: any business screen, renaming the `member` app, the Owner's Staff screens (iam#04) and the Super Admin screens (company#02, company#04).

## 3. Approach
The existing login redirect decides the app from the role; `OWNER` and `STAFF` are added and sent to the `member` app (company spec Q13). The company name comes from `companies/current` once after login and is kept in memory only. Screen states. Login: loading, wrong credentials (generic text), company inactive (catalogue text), success. Header: loading (placeholder), error (app name only), success (company name).

## 4. Tasks (at most 8)
- [x] 1. Add `OWNER` and `STAFF` to the shared role model and the role-to-app routing; the `member` app guards accept them, the `admin` app does not.
- [x] 2. Show the catalogue message for `iam.auth.company_inactive` on the login page (i18n key, no hard-coded text); the field label reads "User ID".
- [x] 3. `CompanyService` in `libs/api-services` for `GET companies/current`.
- [x] 4. The `member` layout header shows the company name (states above).
- [x] 5. `npx nx run-many -t build` and `-t lint`: 0 new errors compared with the base branch.
- [ ] 6. Verify every row of section 5 against the running API and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (what was seen) |
|---|---|---|
| iam `R63`, `AC33` (message) | 2 | Inactive company, correct password -> the login page shows the catalogue text; wrong password -> the generic text **Built:** i18n key `errors.iam.auth.company_inactive` added to the three catalogues; the login page already shows the catalogue text of the API's error code. **NOT seen:** the screen was not opened in a browser. |
| iam `R64`, `AC35` (field) | 2 | The login form asks for a user id; an email typed there gives the generic error **Built:** label and validation text read "User ID" (keys `login.fields.userId`, `login.validation.userIdRequired`). **NOT seen:** the screen was not opened in a browser. |
| iam `R60` (routing) | 1 | Owner and Staff login -> `member` app URL; Super Admin login -> `admin` app URL; Owner opening the `admin` URL -> unauthorized page **Built:** `OWNER` and `STAFF` added to `APP_ROLES`; they are not staff roles, so the existing role-to-app routing sends them to the `member` app and the `admin` guard refuses them. **NOT seen:** the screen was not opened in a browser. |
| company `R10`, `AC4` (UI part) | 3, 4 | Owner login -> the top bar shows the company's name; with `companies/current` failing -> the app name only **Built:** `MemberLayoutComponent` reads `companies/current` once: placeholder `...` while loading, the company name on success, empty on error. **NOT seen:** the screen was not opened in a browser. |

## 6. Not verified
**Checked (2026-11-14):** `npx nx run-many -t build` -> all three apps build; `npx nx run-many -t lint` -> no finding in any new or changed file (the 6 `Static imports of lazy-loaded libraries` errors in the layouts and account pages are identical on `main`); every endpoint this screen calls was exercised against the running API in the backend plans. **NOT seen:** the screen itself was not opened in a browser (no browser tool in this session).

Still open before this plan can be closed: task 6 (open every screen against the running API and record what was seen).

## 7. Risks and notes
The role strings of the UI must match the API's `OWNER` and `STAFF`. Lint findings in files this plan does not touch are not part of it.
