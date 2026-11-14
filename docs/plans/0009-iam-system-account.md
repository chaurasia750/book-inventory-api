# Plan 0009: iam - hidden system account

- **Number:** 0009
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R42-R52 ; AC17-AC23 ; depends on Plans 0003, 0004, 0007
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

The single hidden Super Admin used to verify deployed builds exists, works like a Super Admin, appears nowhere, and carries the owner-approved safeguards.

## 2. Out of scope

Money-action enforcement inside `finance` (this plan only provides the policy `finance` will use), 2FA (deferred), managing the account from any UI/API (forbidden by R42).

## 3. Approach

**Api:**
- `SystemAccountOptions` (`Enabled` default true, `Username`, `Password`, `AllowedIps`, `AlertEmail`, `AllowFinancialActions` default true) validated at startup (R54); credentials only from configuration/secret store (R51).
- `SystemAccountSeeder`: at startup creates or updates the account (`is_system_account = true`, role `SUPER_ADMIN`, placeholder non-deliverable email/mobile that cannot collide with real ones, R52, `must_change_password = false`, hash re-computed when the secret changes); does nothing when disabled and never appears through `IUserDirectory` (filter from 0003).
- Login rules: disabled -> same generic failure as an unknown user (R46); `AllowedIps` check against the trusted client IP (R47); session hard-capped at 60 minutes, no refresh past it, no remember-me (R50); token carries `sys = 1` (R44); forgot/reset never applies (R45); a warning log line on every successful login and an email through `IEmailSender` when `AlertEmail` is set.
- `IActorLabelResolver` implementation returning the fixed label "System" for the account in audit/login-log projections (R48); the real id stays in the database.
- Authorization policy `FinancialAction` (denies the system account when `AllowFinancialActions` is false) exported for `finance` (R49).
- Reserved usernames enforced (already listed in 0003); no endpoint can create/edit/suspend/reset/delete it (the account is invisible to the user directory and commands look users up through it).

**Ui:** none (the "System" label is rendered from the API data in the login-log screens built in 0007).

## 4. Tasks

- [x] 1. Options, seeder, placeholder identity, reserved names check
- [x] 2. Login rules: enabled/allowed IPs/60-minute cap/`sys` claim/alert
- [x] 3. `IActorLabelResolver` + `FinancialAction` policy
- [x] 4. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27 against a throw-away database, the API started from a separate Release build with `Auth:Password:Iterations=1000` (Plan 0012) and `SystemAccount:Password` from the environment. Build 0 warnings, 0 errors. A temporary probe controller with `[Authorize(Policy = "FinancialAction")]` stood in for a `finance` endpoint and was deleted again. Script `api-check9.sh` is not committed. No migration: the columns (`is_system_account`) and the hidden-account filter existed since Plan 0003.

| Rule / AC | Input | Observed |
| --- | --- | --- |
| R42, R51, R52 | startup with `SystemAccount:Password` set | one row: `system`, `SUPER_ADMIN`, `ACTIVE`, `is_system_account = t`, `must_change_password = f`, e-mail `system-account@book-inventory.invalid`, no mobile; log "Seeded the system account" |
| AC17, R44 | login as `system`; decode the token; call `GET users` with it | `200`; claims `role SUPER_ADMIN`, `sys "1"`, `pwd_change "false"`; `GET users` `200` |
| AC17, R43 | admin: `GET users`, `?search=system`, `?role=SUPER_ADMIN`, `GET users/{sysId}`, an unknown id, `GET login-logs`, `login-logs/export` | total 1 (only the visible admin); search 0 rows; role filter 1 row; `404 iam.user.not_found` for the id and for a random id; the login log row is `isSystem: true` with no user name and no id anywhere in the body; the CSV writes `System`, no `system` user name, no id |
| AC18, R42 | admin against the system id: status-history, sessions, `PUT`, approve, activate, deactivate, suspend, reject, change-role, unlock, force-logout, force-password-change, reset-password, release-identity, delete a session | none succeeds and the row is unchanged (`ACTIVE`, `SUPER_ADMIN`, `System`): lookups through the directory answer `404 iam.user.not_found` (status-history, sessions, force-logout, reset-password, delete session), the aggregate's guard answers `403 iam.system_account.immutable` for the others |
| AC20, R5 | the system account deactivates the only visible Super Admin | `409 iam.user.last_super_admin`; the admin stays `ACTIVE` |
| AC21, R49 | probe `GET probe/financial` with default settings; then with `SystemAccount:AllowFinancialActions=false` | default: system token `200`, admin token `200`; switched off: system token `403`, its `GET users` still `200`, admin token `200` |
| AC22, R48 | its login row | `SUCCESS`, `user_id` = the real id, address `::1`; the API shows it as `System` (AC17 row) |
| AC23, R50 | login with `rememberMe: true`; refresh; the session's expiry pushed into the past; refresh again | the session row: `is_remember_me = f`, 60 minutes long; a refresh answers `200` and the expiry stays 60 minutes after creation (no extension); then `401 iam.session.expired`. The log has "Security: the hidden system account signed in from ::1 (session ...)"; with `AlertEmail` set a `SYSTEM_ACCOUNT_LOGIN` notification to that address is queued and `SENT` by the logging sender |
| R45 | forgot-password with its placeholder e-mail; change-password with its token | `202` and no `PASSWORD_RESET` notification; `403 iam.system_account.immutable` |
| R45, R9 | five wrong passwords, then the right one | `423 iam.auth.account_locked` (lockout applies) |
| R51 | restart with a new `SystemAccount:Password` | the old secret `401`, the new one `200`, still exactly one system account |
| R54, R50 | restart with the password `weak`; with `AllowedIps__0=not-an-ip` | the process stops: "is not usable: too_short missing_digit missing_uppercase" and "...not an IP address"; `/health` never answers |
| AC19, R46 | `Enabled=false`: login as `system` and as an unknown user | both `401 iam.auth.invalid_credentials`, both logged `FAILED_UNKNOWN_USER` |
| AC19, R47 | `AllowedIps=[1.2.3.4]`; then `[1.2.3.4, ::1, 127.0.0.1]` | `401 iam.auth.invalid_credentials`; `200` |

**NOT verified:** the alert mail through a real SMTP server (only the logging sender); `AllowedIps` behind a real reverse proxy (only the socket address, no forwarded headers); `AllowFinancialActions` on a real `finance` endpoint (none exists yet - only the probe); the `SystemAccountMinimumLength` setting (present, not exercised).

## 6. Risks and notes

- Accepted owner risks (always-on in production, full money power, no 2FA) are documented in the spec; the compensating controls are exactly what this plan verifies. Release checklist (Plan 0010) reminds to review the secret and optional IP allowlist before go-live.
