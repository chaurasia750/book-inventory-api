# Release checklist (IIS, same-origin hosting)

Owner: whoever deploys. Every line is a yes/no; a "no" stops the release until the owner accepts it in writing.
Background: [platform spec](specs/platform.md), Plan 0010, [iam spec](specs/iam.md).

## How it is hosted

- One deployable: `scripts/publish.ps1` builds the three Angular apps (production configuration, base href `/`, `/admin/`, `/member/`), publishes the API and puts the apps in `wwwroot`. The API serves the apps and `/api/v1` from **one origin**, because the refresh cookie is `SameSite=Strict`.
- Locally nothing changes: the API runs alone (`dotnet run`) and each UI app runs on its own `nx serve`, which proxies `/api` to the API (`proxy.conf.json`). The API serves a UI only when a `wwwroot` with an `index.html` exists next to it.
- IIS: ASP.NET Core Module, in-process, application pool "No Managed Code". `web.config` is part of the publish output.

## Before the release

- [ ] `dotnet build book-inventory.slnx` shows 0 warnings, 0 errors on the commit being released.
- [ ] Every migration of the release has been read (generate the SQL) and its effect understood. The start itself brings the schema up to date (`Database:MigrateOnStartup`, on by default in every environment, spec platform P16); the person reviews the generated SQL and the rollback note of any migration that drops or rewrites data before the release, and sets `Database__MigrateOnStartup=false` if the step must instead be run by hand.
- [ ] `scripts/publish.ps1 -Output <folder> -Clean` finished without error; `<folder>\wwwroot\index.html`, `admin\index.html` and `member\index.html` exist.

## Secrets and settings (environment variables of the application pool, never files)

- [ ] `ConnectionStrings__Default` points at the production database.
- [ ] `JwtSetting__IssuerSigningKey` is a new random key of at least 32 bytes, different from every other environment.
- [ ] `Seed__SuperAdmin__UserName`, `__Email`, `__FullName`, `__Password` are set for the first start only. After the first login the Super Admin has changed the password, and the `Seed__*` variables are removed.
- [ ] `Auth__Reset__LinkUrl` is `https://<production host>/auth/reset-password` (startup fails without it).
- [ ] `Auth__Password__Iterations` is **unset** (default 210000) or higher. The API logs a warning at startup when it is lower.
- [ ] `Email__Host` (with `Email__FromEmail`, credentials) is set to the real SMTP server, **or** the owner accepts that mails are only logged (`LoggingEmailSender`): without SMTP nobody receives a password-reset link.
- [ ] System account (hidden Super Admin, spec iam R42-R52): `SystemAccount__Password` is a long random secret held by the owner, or the account is switched off with `SystemAccount__Enabled=false`. Decide `SystemAccount__AllowedIps` (an allow-list), `SystemAccount__AlertEmail` and `SystemAccount__AllowFinancialActions` (R49) deliberately; the defaults are: any IP, no alert mail, money actions allowed.
- [ ] `Registration__Mode` is the intended one (`ApprovalRequired` is the default; `InstantLogin` signs agents in without approval).
- [ ] `ReverseProxy__KnownProxies` lists the proxy address if the API sits behind a reverse proxy or load balancer (empty is right for IIS in-process on the same host, where the socket address is the client).
- [ ] `AllowedHosts` is narrowed to the production host name if the site is bound to more than one name.

## Network

- [ ] The site is bound to HTTPS with a valid certificate; plain HTTP only redirects (the API redirects to HTTPS and sends HSTS outside Development).
- [ ] The application pool identity can write the `Logs` folder next to the application (Serilog file sink) and read the application folder.
- [ ] The database is reachable only from the application server.

## After the release (smoke test on the real host name)

- [ ] `GET /health` answers `200`.
- [ ] `/` shows the public site; `/login` works; `/admin/dashboard` and `/member/dashboard` open and, when signed out, redirect to `/login`; reloading a deep link keeps the user signed in.
- [ ] `GET /api/v1/does-not-exist` answers `404` (not an HTML page).
- [ ] The response headers of an HTML page carry `Content-Security-Policy`, `X-Content-Type-Options`, `X-Frame-Options`, `Referrer-Policy`, and `Strict-Transport-Security`.
- [ ] The sign-in sets `book_inventory_refresh` with `HttpOnly; Secure; SameSite=Strict` and path `/api/v1/auth`.
- [ ] The first Super Admin signs in, is forced to change the password, and the seed variables are gone.
- [ ] The application log shows no warning about a low `Auth:Password:Iterations`.

## Rollback

1. Keep the previous publish folder. To roll back, stop the site, point IIS at the previous folder (or copy it back) and start the site.
2. Database: migrations are forward-only. Before applying a migration take a backup; a migration that drops or rewrites data has its own rollback note in its plan. Restore the backup only after the owner decides; never `database update 0`.
3. Rotate the JWT signing key or the system-account secret if either may have leaked (all sessions end when the key changes).

## Known limits of this hosting (accepted)

- Single instance: the session cache and the background workers live in one process. Two instances would need a shared cache and a job lock.
- With `Cors:AllowedOrigins` empty (the production default) the API sends no CORS header at all (Plan 0015); same-origin calls need none. Set it only when another origin must call the API.
