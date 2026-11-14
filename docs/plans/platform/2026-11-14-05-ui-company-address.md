# Plan platform#05: UI company address (`/<slug>/...` instead of `/member`)

- **File:** `docs/plans/platform/2026-11-14-05-ui-company-address.md`
- **Kind:** UI + hosting (repository `book-inventory-ui`; hosting in `book-inventory-api` `SpaHosting`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`, API build `dotnet build book-inventory.slnx`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U12; UAC8 (and company R15)
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** company#05 (Done)
- **UI plan:** this is itself the UI plan; the hosting part is small and listed here
- **Approved by / on:** abhishek / 2026-11-14 (chat answers: `/abc-book-center/schools`, slug from the name, wrong slug redirects; "change password bhool mat jana")
- **Created:** 2026-11-14
- **Base branch:** `feature/school-screens-ui` (UI repo) and `docs/school-master` (API repo), owner's standing instruction

## 1. In one minute
After this plan the company's app opens at `/abc-book-center/...` instead of `/member/...`. Sign in as an Owner and the address bar shows the company name; `/abc-book-center/change-password` works like every other page.

## 2. Touches / leaves out
Touches: `apps/member/src/index.html` and a tiny `base-href.js` (the app's base address is the first address segment), the member route tree (a guard that checks the slug), the login redirect and the role-to-app table (`libs/constants`, `libs/auth`, `libs/shared-models`, public-site login), `scripts/serve-single.mjs`, `scripts/production-deploy.js`, `scripts/iis-web.config`, and in the API repo `Hosting/SpaHosting.cs` (company addresses served as the member app).
Leaves out: a stored slug, redirects from an old slug after a rename, any route inside the app (they stay `/schools`, `/staff`, ...), the `nx serve member` dev server (it still opens at `/`).

## 3. Approach
The router's base address is dynamic: `base-href.js` (an external script, because the API's content-security policy forbids inline scripts) writes `<base href="/<first segment>/">`. All existing routes, links and guards keep working under that base. A `companyAddressGuard` on the layout route asks `GET companies/current`; when the slug differs it replaces the address with the right one (a full navigation, because the base address changes). Hosts map every non-reserved first segment to the member app.

## 4. Tasks (at most 8)
- [x] 1. `base-href.js` and the member `index.html` use it instead of the fixed base (U12).
- [x] 2. `companyAddressGuard` on the member layout route; Super Admin or no company goes to `/admin` (U12).
- [x] 3. Sign-in redirect for company accounts opens `/<slug>/`; `APP_URLS.MEMBER` stays only as the fallback address when the slug cannot be read (U12).
- [x] 4. `scripts/serve-single.mjs`, `production-deploy.js` (member without a fixed base href) and `iis-web.config` map company addresses to the member app (U12).
- [x] 5. API host: `UseCompanyAddresses` in `SpaHosting` (U12).
- [x] 6. Builds and lint of both repositories: 0 new findings.
- [ ] 7. Verify UAC8 in the running single-port server and record what was seen; the screens in a browser are the owner's check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U12`, `UAC8` | 1-5 | HTTP: `GET /abc-book-center/schools`, `/abc-book-center/main-*.js`, `/admin`, `/login` on the single-port server |
| `U12`, `UAC8` | 2, 3 | Owner sign-in opens `/abc-book-center/`; wrong slug lands on the own slug; `/member/...` lands on the own slug (browser: owner's check) |

## 6. Not verified
- Observed (2026-11-14, single-port server `node scripts/serve-single.mjs` on 4200, API on 7100): `/abc-book-center/schools`, `/abc-book-center/` -> 200 HTML of the member app (contains `base-href.js`); `/abc-book-center/main-*.js` -> 200 JavaScript; `/abc-book-center/assets/i18n/en.json` -> 200; `/member/base-href.js` and `/member/schools` -> 200; `/admin/`, `/login`, `/auth/forgot-password` -> the other apps. `GET /api/v1/companies/current` through the proxy as Super Admin entered in each company -> slugs `abc-book-center` and `children-book-center`.
- NOT verified: the sign-in redirect, the address guard and a reload at `/<slug>/...` in a browser (task 7, the owner's check); the API host mapping `UseCompanyAddresses` (the API served no `wwwroot` here); the IIS rules in `scripts/iis-web.config` (no IIS here); `nx lint` shows no new finding in the changed files.

## 7. Risks and notes
A company named `Member` would have `/member` as its address, which is also the old address; both lead to the same app, which then redirects, so it works. The IIS rules and the API host mapping are not exercised here (no IIS; the API serves the UI only when `wwwroot` exists).
