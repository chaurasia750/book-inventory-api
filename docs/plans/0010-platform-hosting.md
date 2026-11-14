# Plan 0010: platform - same-origin hosting (API serves the UI) and release checklist

- **Number:** 0010
- **Module:** `platform` (primary) ; also touches: `iam` (cookie/CORS settings)
- **Spec:** [docs/specs/platform.md](../specs/platform.md) - P13, UAC1-UAC3 ; [iam.md](../specs/iam.md) D3
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

Development and production behave the same way: the browser only ever talks to **one origin** (in production the API serves the UI; in development the UI dev-server proxies `/api` to the API), because the refresh cookie is `SameSite=Strict`. One deployable on Windows: the ASP.NET Core API serves the three Angular apps and the `/api/v1` endpoints from the same origin (under IIS or Kestrel), with production-safe cookie, HTTPS, proxy and CORS settings, a repeatable deploy script, and a written release checklist.

## 2. Out of scope

Provider-specific email, CI/CD servers, load balancing, monitoring, backups, TLS certificate procurement.

## 3. Approach

**Api:**
- Static file hosting and SPA fallbacks: `public-site` at `/`, `admin` at `/admin`, `member` at `/member` (each with its own `index.html` fallback so deep links reload); API under `/api/v1`; `/health` and OpenAPI unchanged; UI assets get long-lived cache headers and `index.html` no-cache.
- Production hardening: HTTPS redirection and HSTS, `Secure` cookies, `SameSite=Strict`, forwarded headers only from configured trusted proxies (IIS on the same host by default), CORS not needed in either mode (same origin; the development `Cors:AllowedOrigins` entry stays only for tools that call the API directly), security headers (CSP suited to Angular, `X-Content-Type-Options`, `Referrer-Policy`, `X-Frame-Options`).
- IIS: `web.config` for the ASP.NET Core Module (in-process), environment variables for secrets (`ConnectionStrings__Default`, `JwtSetting__IssuerSigningKey`, `Seed__SuperAdmin__*`, `SystemAccount__*`), app pool notes; appsettings.Production.json without secrets.
- `publish` script: builds the UI apps, copies them into the API's `wwwroot`, publishes the API to a folder ready for IIS.

**Ui:**
- **Development same-origin proxy (found 2026-09-27):** `nx serve` runs the UI on `http://localhost:4200` and the API is `https://localhost:7100`. Browsers treat `http://localhost` and `https://localhost` as cross-site (schemeful same-site), so the `SameSite=Strict` refresh cookie is dropped: login works, but every refresh answers `401 iam.session.not_found` (the cookie never arrives), so a reload or a hard navigation between apps logs the user out. Fix without weakening the cookie: a shared `proxy.conf.json` (`/api` -> `https://localhost:7100`, `secure: false` for the dev certificate) set as `proxyConfig` on the `serve` target of the three apps, and `DEV_ENVIRONMENT.apiUrl` becomes `/api/v1` like staging and production. The apps on other dev ports share the cookie (cookies are not port-scoped), so a hard navigation between `4200`, `4201` and `4202` keeps the session. The absolute `loginUrl` for the separate dev ports stays.
- Build configurations produce `base-href` per app (`/`, `/admin/`, `/member/`) and use the `/api/v1` relative URL; the legacy `production-deploy.js`/`serve-single.mjs` are replaced by the single publish script and a small local single-port runner for smoke tests if still useful.

**Release checklist (docs):** `docs/Release_Checklist.md`: `Auth:Password:Iterations` unset or 210000 (Plan 0012), migration reviewed and applied by a person, secrets set, first Super Admin seeded then password changed, `Email:Host` set (the core logging sender, spec core N12, replaced by a real SMTP provider) or knowingly accepted, system-account secret/IP allowlist reviewed, HTTPS bound, health check, log folder writable, rollback steps.

## 4. Tasks

- [x] 1. API static hosting + SPA fallbacks + cache headers
- [x] 2. Production hardening (HTTPS/HSTS, cookies, forwarded headers, CORS dev-only, security headers)
- [x] 3. IIS `web.config`, production appsettings, environment-variable mapping
- [x] 4. Publish script (`scripts/publish.ps1`) that builds the UI with per-app base href and publishes the API with the apps in `wwwroot`. Owner decision 2026-09-27: hosting the UI from the API is for IIS deployment only; locally the API and each UI app run separately on their own ports. The legacy single-port scripts of the UI repo (`serve-single.mjs`, `production-deploy.js`) are therefore left in place (see Backlog B41)
- [x] 4b. UI development proxy (`proxy.conf.json`, `proxyConfig` on the three `serve` targets, `DEV_ENVIRONMENT.apiUrl = /api/v1`) so `nx serve` is same-origin like production
- [x] 5. `docs/Release_Checklist.md`
- [x] 6. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27. Build 0 warnings, 0 errors. Scripts `ui-real6.sh`, `run9.mjs`, `api-check11.sh`, `run10.mjs` are not committed. No migration.

**Development proxy (task 4b)** - API on `https://localhost:7199` with a throw-away database, `nx serve public-site --port 4290` with the committed `proxy.conf.json` copied and its target changed to 7199 (the committed file targets 7100; the mechanism is the same). Edge on `http://localhost:4290`:

| Step | Observed |
| --- | --- |
| log in as the Super Admin | `POST /api/v1/auth/login 200`; the cookie list has `book_inventory_refresh` with `HttpOnly`, `Secure`, `SameSite=Strict`, path `/api/v1/auth` on the dev origin |
| `POST /api/v1/auth/refresh` through the dev proxy | `200` |
| open `/login` again (session hint set) | the page itself calls `POST /api/v1/auth/refresh` and gets `200` (before this change the same call answered `401 iam.session.not_found`, seen 2026-09-27) |

**Published output on Kestrel, `ASPNETCORE_ENVIRONMENT=Production`** - `scripts/publish.ps1` (three production UI builds + `dotnet publish`), then `dotnet BookInventory.Api.dll` from the output folder on `https://localhost:7199` (the local dev certificate) and `http://localhost:5299`, secrets from environment variables, a throw-away database:

| Check | Observed |
| --- | --- |
| `GET /`, `/login` | `200`, `<base href="/">`, `Cache-Control: no-cache` |
| `GET /admin/dashboard`, `/member/dashboard` | `200`, `<base href="/admin/">` and `<base href="/member/">` |
| security headers on the HTML page | `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy: strict-origin-when-cross-origin`, `Content-Security-Policy: default-src 'self'; script-src 'self' 'unsafe-hashes' 'sha256-...'; style-src 'self' 'unsafe-inline' ...` |
| hashed file `/main-L5Y62MIK.js` | `200`, `Cache-Control: public,max-age=31536000,immutable`; `/assets/i18n/en.json` `no-cache` |
| `GET /nope.js`; `GET` and `POST /api/v1/does-not-exist` | `404`; `404` with no HTML content type; `404` |
| `GET /health`; `GET /openapi/v1.json` | `200`; `404` (OpenAPI only in Development) |
| `http://localhost:5299/login` | `307` to `https://localhost:7199/login` |
| HSTS | on the host name `book-inventory.test` (curl `--resolve`): `Strict-Transport-Security: max-age=2592000`; on `localhost` none (the framework excludes it by design) |
| login cookie | `Set-Cookie: book_inventory_refresh=...; path=/api/v1/auth; secure; samesite=strict; httponly` |
| CORS, request with `Origin: https://evil.test` | **not** "no CORS headers": `Access-Control-Allow-Origin: *` and no `Access-Control-Allow-Credentials` - the platform policy with an empty `Cors:AllowedOrigins` allows any origin without credentials (see Backlog B40) |
| Edge on the single origin (UAC2, UAC3) | the login page renders; login lands on `/admin/dashboard`; `/admin/users` and `/admin/profile` reload and stay signed in; no token in `localStorage`/`sessionStorage` (only the `rrj.session` hint); in a fresh browser context `/admin/dashboard` and `/member/overview` redirect to `/login?returnUrl=...`; **0** Content-Security-Policy violations in the console across those pages |
| publish output | `wwwroot` holds the public site, `admin/` and `member/` with no `web.config` inside; `web.config` (in-process ASP.NET Core Module) and `appsettings.Production.json` at the root |

**NOT verified:** IIS itself (no IIS on this machine; `web.config` and the application-pool notes are untested); a publicly trusted certificate and HSTS on a real host; the CSP on the public pages that load images from other hosts (only login, admin and the stand-in flows were opened); `nx serve` of the `admin` and `member` apps (same option as public-site); a hard navigation between separate dev ports.

## 6. Risks and notes

- The dev proxy assumes the API dev URL `https://localhost:7100` (from `launchSettings.json`); a developer who runs the API on another port changes it in `proxy.conf.json` only. `SameSite=None` in Development was rejected: it weakens the cookie and makes development behave differently from production.
- CSP for Angular needs care with inline styles; it is tuned against the real build and any relaxation is listed.
- Single-instance assumption (in-memory session cache, hosted sweepers); a multi-instance deployment would need a shared cache and a job lock: noted for later.
