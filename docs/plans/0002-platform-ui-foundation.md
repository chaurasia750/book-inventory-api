# Plan 0002: UI foundation - adapt the existing UI's API layer to the new backend

- **Number:** 0002
- **Module:** `platform` (primary) ; also touches: none
- **Spec:** [docs/specs/platform.md](../specs/platform.md) §10 - U1-U8, UAC1-UAC4 ; [iam.md](../specs/iam.md) D2, D3, D6, D16
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-26

## 1. Goal

The existing Angular workspace (`book-inventory-ui`: three apps, all pages, all libraries) stays exactly as it is. Only the layer that talks to the backend is adapted to the new PostgreSQL API: environment/base URL, the authentication flow, interceptors, the models the auth flow uses, guards and the message catalogue. Screens that belong to modules whose new API does not exist yet are **hidden from navigation, not deleted**; they are switched on one module at a time when that module's API and its adaptation plan are done. All three apps still build and lint, and `/login` renders.

## 2. Out of scope

Deleting or rewriting any business page, component or library (owner decision, section 6); adapting the business pages' own services to the new endpoints (each module's plan does that); the real IAM screens' polish (Plans 0004-0007); hosting (Plan 0010).

## 3. Approach

**Api:** none (the UI is written against the contract in [iam.md §5](../specs/iam.md)).

**Ui - changed:**
- `libs/constants`: `environment.const.ts` (API base URL `/api/v1` in production, the API's dev URL otherwise, optional absolute `loginUrl` for the separate dev ports; cookie-name settings no longer needed), `APP_NAME`/brand placeholder, a small `FEATURES` switch (`iam: true`, every legacy module `false`) used by navigation.
- `libs/shared-models`: add `Role` strings (`SUPER_ADMIN`, `SALES_CRM`, `ACCOUNTS`, `MEMBER`), `AuthUser`, login/refresh/password request-response types and `ProblemDetails`. Existing models stay for the legacy pages.
- `libs/auth`: `AuthService` rewritten internally: access token in memory only, session restored at start-up through the refresh cookie (`initializeSession`), single-flight refresh, login/logout/change/forgot/reset calls to the new endpoints, a non-secret session hint flag only. Its **public surface the rest of the UI already uses stays** (`isAuthenticated`, `roles()` mapping staff roles to the existing `admin` bucket and `MEMBER` to `member`, `userId`, `hasRole`, `logout`, `currentUser`) so no page has to change; the new `role()` signal exposes the real role. The old cookie-config/session-expiry/preferences services are removed only if nothing else uses them.
- `libs/interceptors`: `authInterceptor` (Bearer from memory, 401 -> one refresh -> retry, else back to login); `toastInterceptor` becomes the error interceptor (problem details `code` -> catalogue text -> toast, `SKIP_ERROR_TOAST` context flag for pages that show errors inline) and **stops unwrapping** the old `{data, errorCode}` envelope (the new API has none).
- `libs/guards`: keep `authGuard`, `roleGuard`, `guestGuard`; role checks use the new role strings; add `mustChangePasswordGuard` (used from Plan 0005); login redirect goes through `AuthService.redirectToLogin`.
- `libs/layout`: header shows the user from `AuthService` instead of the old `/users/info` call; the legacy notification dropdown hidden behind the feature switch; brand shown in header/sidebar is the neutral placeholder.
- `assets/i18n/en.json`: add `errors.*` (by API code and by HTTP status) and `iam.*` keys; the login page's marketing copy keys get neutral text (no structural change).
- Apps: `app.config.ts` of the three apps provide the new environment/interceptors; admin and member layouts filter their `navItems` through the feature switch so only Dashboard (and later the IAM items) show; login, forgot and reset pages are adapted to the new `AuthService` (fields, endpoints, messages) without redesigning them.
- `CLAUDE.md`/`AGENTS.md` of the UI repo: drop the rules that no longer hold (envelope unwrapping, mock role assignment, Binsera cookies) and add the new ones plus the link to the API repo workflow; keep everything else.

**Ui - untouched:** every legacy page, component, service and library not listed above, the legacy assets, the deployment scripts (Plan 0010 replaces them).

## 4. Tasks

- [x] 1. `git init`, `.gitignore` review, snapshot commit + tag `legacy-snapshot` (done 2026-09-26: commit `f3fab7d`, tag `legacy-snapshot`, pushed to `Haut-It-Solution/book-inventory-system-ui`; 674 files, no secrets found by a scan, no file over 6 MB)
- [x] 2. `npm ci` (Node 24) and baseline build of the three apps: all three build successfully in the development configuration
- [x] 3. Models, constants (environment, feature switch), i18n keys
- [x] 4. `AuthService` rewrite keeping its public surface; interceptors; guards
- [x] 5. Header/layout wiring and navigation feature switch; placeholder brand
- [x] 6. Login, forgot and reset pages adapted to the new `AuthService`
- [x] 7. UI `CLAUDE.md`/`AGENTS.md`/`README.md` updates
- [x] 8. `nx run-many -t lint,build`; `nx serve public-site` and load `/login`; record results; set status, add plan links to specs

## 5. Verification

**Baseline (before any change):** `npm ci` (Node 24) and `nx run-many -t build --configuration=development`: all three apps built successfully.

**Build and lint after the change:** `nx run-many -t build --configuration=development` for the three apps passed; the three apps were also built in the `staging` configuration (same-origin `/api/v1`) with `--base-href=/`, `/admin/`, `/member/`. `eslint` on all 31 changed/new `.ts` files: 0 errors, 0 warnings. `nx run-many -t lint` still reports errors only in legacy files this plan did not touch (`secure-img.directive.ts`, `dropdown.ts`, the genealogy components), which existed before.

**UAC1 - nothing deleted:** `git diff --diff-filter=D --name-only legacy-snapshot` lists only the five auth-plumbing files that the new flow replaced: `libs/auth/.../auth-cookie.config.ts`, `auth-session-expiry.service.ts`, `auth-session-preferences.service.ts`, `libs/interceptors/.../toast.interceptor.ts` (now `error.interceptor.ts`) and `libs/shared-models/.../reset-password.model.ts`. No page, business library or service was deleted (an earlier attempt that did delete them was reverted, see section 6).

**Browser run** (real Edge driven by a scratch script, the three built apps served from one origin with a stub of `/api/v1/auth`; all lines PASS):

| Check | Observed |
|---|---|
| `/login` renders | catalogue text "Sign in to your account", neutral "Welcome to BookInventory", no legacy product copy |
| First visit | no `/auth/refresh` call when the browser has no session hint |
| UAC4 wrong password | API code `iam.auth.invalid_credentials` shows "Invalid credentials. Please check your details and try again." inline; nothing stored |
| Login (staff) | redirected to `/admin/dashboard`; shell shows "Asha Verma", "Welcome, Asha Verma", "Super Admin"; legacy menu items absent; the legacy dashboard is not loaded (no legacy endpoint called) |
| UAC2 | storage after login: `{"local":{"rrj.session":"1"},"session":{},"cookie":""}` - no token or credential anywhere the page can read (refresh cookie is HttpOnly) |
| UAC3 reload | `/admin/dashboard` reload stays put; the app called `POST /auth/refresh` with the cookie only and then showed the user again; API calls carry `Authorization: Bearer` from memory |
| UAC3 logout | user-menu "Sign out" -> `/login?loggedOut=1`, session hint removed; a deep link afterwards goes to `/login?returnUrl=...` with no refresh attempt |
| UAC4 toast | a legacy page calling the old API got 404 problem details -> toast "The requested item was not found." (catalogue text by HTTP status) |
| UAC4 inline | forgot-password: 500 without code -> "Something went wrong on our side. Please try again later."; code `iam.password.same_as_current` -> its catalogue text; success text from the catalogue |
| Role routing | an agent (`MEMBER`) lands in `/member` and sees the API user |

**Found and fixed during verification:** (1) injecting `TranslateService` while the interceptor chain is built is a circular dependency (NG0200), so it is resolved lazily; (2) errors arriving while the app is still starting used the fallback text because the catalogue was not loaded yet, so the interceptor waits with `translate.get()`; (3) the auth layout and sidebar still defaulted to the old brand name.

**NOT verified:** behaviour against the real new API (it has no auth endpoints yet: Plan 0004); the development configuration's absolute `loginUrl` across separate ports (only same-origin was exercised); `nx serve` (the built bundles were served instead).

## 6. Risks and notes

- **Owner decision (2026-09-26):** the UI is the user's real product UI that ran against SQL Server; the new project exists only because of PostgreSQL. All UI code is reused later, so **nothing is deleted**. An earlier draft of this plan removed business pages and libraries; that draft was wrong, was reverted from the snapshot and replaced by this one.
- **Explicit permission (owner, 2026-09-26):** the legacy **property/inventory**, **leads** and **commission** UI code may be deleted. IAM does not need them, so this plan leaves them untouched (hidden by the feature switch); they are replaced or removed in the plans of their own modules (`realty` for property and leads, `mlm`/`finance` for commission). Everything else stays.
- Legacy pages keep their old services: until a module is adapted their calls would fail against the new API, which is why they are hidden from navigation by the feature switch instead of removed.
- Branding is a neutral placeholder in the shell only (header, sidebar, login, page titles); the legacy marketing pages keep their content until their module is adapted.
- **Follow-ups noticed:** the shared message catalogue exists as three copies (`assets/i18n`, `apps/public-site/public/assets/i18n`, `apps/public-site/src/assets/i18n`) that this plan kept in step; unify them in Plan 0010. The legacy `admin` `users` page still shows sample data and is not linked from the menu until Plan 0006 replaces it. The legacy asset files (old logos and product images) are untouched and unreferenced by the shell.
- **Follow-ups noticed:** the shared message catalogue exists as three copies (`assets/i18n`, `apps/public-site/public/assets/i18n`, `apps/public-site/src/assets/i18n`) that this plan kept in step; unify them in Plan 0010. The legacy `admin` `users` page still shows sample data and is not linked from the menu until Plan 0006 replaces it. The legacy asset files (old logos and product images) are untouched and unreferenced by the shell.
