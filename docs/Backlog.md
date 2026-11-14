# Backlog: things not verified or left open

Owner rule (2026-09-27): every item a plan could **not verify**, and every known gap or follow-up a review leaves open, is
added here when that plan is closed. The whole list is reviewed **after all plans are implemented** (each item then becomes a
check, a fix in a new plan, an accepted limit, or is dropped). Newest plans last. Status: `open` until reviewed.

Kinds: **NV** not verified · **GAP** known gap · **DEC** decision the owner has to take · **DEBT** cleanup.

## Platform (0001, 0002, 0010 area)

| # | Kind | Item | Source | Status |
| --- | --- | --- | --- | --- |
| B1 | NV | UI `nx serve` (dev server) was never run; only built bundles were served | Plan 0002 | done for public-site in Plan 0010 (admin/member not run) |
| B2 | GAP | Development runs the UI on `http://localhost:4200` and the API on `https://localhost:7100`: cross-site, so the `SameSite=Strict` refresh cookie is dropped and refresh answers `401 iam.session.not_found`. Fix planned in Plan 0010 (dev-server proxy) | review 2026-09-27 | fixed in Plan 0010 (dev proxy) |
| B3 | DEBT | The message catalogue `en.json` exists as three copies (`assets/i18n`, `apps/public-site/public/assets/i18n`, `apps/public-site/src/assets/i18n`) kept in step by scripts; unify | Plan 0002 | planned (0010) |
| B4 | DEBT | `AddValidatedOptions` in BuildingBlocks does not validate nested option objects; `AuthOptions` validates itself. Fix once in the library (a platform plan) | Plan 0003 | open |
| B5 | DEC | BuildingBlocks JSON enum converter was changed to `SnakeCaseUpper` (one line in `FilterExtension.cs`) as part of Plan 0004; a cross-cutting change that deserves an explicit review | Plan 0004 | approved 2026-09-27 (permanent wire format) |
| B6 | DEBT | `AuthenticationProblemFactory` repeats the ProblemDetails + code + generic-text logic of `GlobalExceptionHandler`; a shared factory belongs in BuildingBlocks | review 2026-09-26 | open |
| B7 | DEBT | Pre-existing lint errors in legacy UI files (`secure-img.directive.ts`, `dropdown.ts`, genealogy components) and 18 lint warnings in legacy admin files | Plans 0002, 0007 | open |

## iam (0003 - 0007, 0012)

| # | Kind | Item | Source | Status |
| --- | --- | --- | --- | --- |
| B8 | NV | Real reverse proxy with `ReverseProxy:KnownProxies` set (only "no proxy configured" was verified) | Plan 0004 | open |
| B9 | NV | `refresh` from two tabs at the same instant (the second use of a just-rotated token revokes the session by design, R13) | Plan 0004 | open |
| B10 | DEC | The refresh session lifetime is sliding with no absolute cap (spec silent) | Plan 0004 | decided and done in Plan 0014 (30-day cap) |
| B11 | NV | The 5-minute timers of `ExpiredSessionSweeper` and `SuspensionLiftSweeper` (their logic runs through the lazy paths that were verified) | Plans 0004, 0006 | open |
| B12 | NV | Concurrent first startup of two separate API processes seeding the Super Admin (checked with two scopes in one process) | Plan 0003 | open |
| B13 | GAP | Two Super Admins deactivating each other at the same moment can both pass the last-admin check (R5, check-then-act) | Plan 0006 | fixed in Plan 0013 (advisory lock) |
| B14 | NV | R5 last-Super-Admin refusal through the API (unreachable with an active Super Admin as the actor; verified in the domain in Plan 0003) | Plan 0006 | open |
| B15 | NV | Remaining admin action dialogs in a browser (reject, deactivate, change role, unlock, force logout, force password change, reset link, release identity, revoke session); verified through the API, they share one dialog | Plan 0006 | open |
| B16 | NV | The member app's profile, sessions and login-history screens in a browser (shared components; built and linted only, no agent user in the check database) | Plan 0007 | open |
| B17 | NV | "Sign out everywhere" button in a browser (API verified in Plan 0004) | Plan 0007 | open |
| B18 | NV | CSV export at exactly the row cap boundary (10,000) | Plan 0007 | open |
| B19 | GAP | The user dropdown's "Signed in" time is the session's login time, not the previous login | Plan 0007 | open |
| B20 | GAP | `AppUser` is split into three partial files (497 lines in total); the "class at most 300 lines" rule is met per file only | Plan 0003 | open |
| B21 | GAP | `UserDirectory.SearchAsync` orders through the platform `OrderBy(string)`, which accepts any entity property (for example `PasswordHash`); the new login-log search uses its own whitelist. Restrict the user list the same way | review 2026-09-27 | fixed in Plan 0013 (whitelist) |
| B22 | GAP | The user search does not escape `%` / `_` in the search text (harmless for a substring search) | Plan 0003 | open |
| B23 | NV | PBKDF2 at the upper bound (2,000,000 iterations would make each login very slow; the bound only guards against a typo) | Plan 0012 | open |
| B24 | NV | Temporary password shown once: the copy button relies on `navigator.clipboard` (secure contexts only) | Plan 0006 | open |
| B25 | GAP | Creating an account with a duplicate value races two requests past the lookup; the unique index then answers 500 instead of 409 | review 2026-09-27 | checked in Plan 0013: answers 409 (generic text), not 500; field code is B43 |

## core notifications (0011)

| # | Kind | Item | Source | Status |
| --- | --- | --- | --- | --- |
| B26 | NV | A real SMTP server (no provider yet, spec CO1); the MailKit sender is configured but never connected | Plan 0011 | open |
| B27 | NV | Two separate API processes running the notification worker at once (the race was run inside one process) | Plan 0011 | open |
| B28 | GAP | A worker that dies mid-send can make a mail go out twice after `StuckAfterMinutes` (at-least-once delivery) | Plan 0011 | accepted risk, review |
| B29 | DEBT | `NotificationWorker` still has its own loop; the two sweepers use `PeriodicSweeper` | Plan 0006 | open |
| B30 | DEC | Notification admin screen, template editing, retry UI (spec CO3) and SMS/WhatsApp (CO2) are not planned | core spec | open |
| B31 | GAP | The forced-change filter (R12) allow-list matches the path suffix; a future controller ending in `/auth/me` would also pass | review 2026-09-26 | fixed in Plan 0013 (exact route) |

| B32 | NV | The system-account alert mail through a real SMTP server, `AllowedIps` behind a real reverse proxy, and `SystemAccount:Password` at `Auth:Password:SystemAccountMinimumLength` (setting exists, not exercised) | Plan 0009 | open |
| B33 | GAP | `FinancialAction` was proven only with a temporary probe endpoint; `finance`/`mlm` must put `[Authorize(Policy = AuthPolicies.FinancialAction)]` on every money-moving and approval endpoint (R49) | Plan 0009 | open (finance plan) |
| B34 | DEC | Management commands against the hidden account's id answer a mix of `404 iam.user.not_found` and `403 iam.system_account.immutable`; only `GET users/{id}` is required to be 404 (R43). Make every command 404 if the account's existence must not leak to a Super Admin who knows the id | Plan 0009 | decided and done in Plan 0014 (all 404) |
| B35 | NV | The real agent registration form and the `mlm` endpoint that calls `IMemberUserRegistration`; the hand-off was proven with a temporary stand-in only. The legacy sign-up page still posts to the old `/members/registration` and has to be switched to `RegistrationHandoffService` | Plan 0008 | open (mlm registration plan) |
| B36 | GAP | Mobile numbers are compared digits-only, so `+91 98765 00001` and `98765 00001` count as two different numbers and both can register. Decide whether a country code is stripped or a fixed length is required | Plan 0008 | fixed in Plan 0013 (prefix stripped) |
| B37 | DEC | `Approve` sets `must_change_password = true`, so an agent who chose their own password at registration must change it at the first login after approval (only `InstantLogin` skips it, R53) | Plan 0008 | decided and done in Plan 0014 |
| B38 | NV | IIS itself (`web.config`, in-process hosting, application-pool identity and log folder rights) - no IIS on the development machine | Plan 0010 | open |
| B39 | NV | CSP checked only on the login, admin and stand-in pages; the public marketing pages (external images) and `nx serve` of the admin and member apps were not opened | Plan 0010 | open |
| B40 | DEC | With `Cors:AllowedOrigins` empty the platform policy answers `Access-Control-Allow-Origin: *` (no credentials). Same-origin needs no CORS; decide whether the library should send no CORS headers at all when no origin is configured (BuildingBlocks change) | Plan 0010 | decided and done in Plan 0015 |
| B41 | DEBT | The UI repo keeps its legacy single-port scripts (`serve-single.mjs`, `production-deploy.js`, `iis-web.config`, `deploy*`/`start:single*` npm entries); `deploy:api` still points at the old sibling API. Replaced in practice by `scripts/publish.ps1`; remove when the owner agrees | Plan 0010 | open |
| B42 | GAP | In development the apps are separate dev servers, so the post-login hard redirect to `/admin` or `/member` only works when the app is served under that path (`serve:single`) - not on a lone `nx serve public-site` | Plan 0010 | open |
| B43 | GAP | A race on a unique value answers `409` with the platform's generic text and no `iam.user.duplicate_*` code (the lookup path has the code); a BuildingBlocks handler change would add it | Plan 0013 | open |

## To add when new plans close

Every further plan appends its own rows here.
