# Plan 0007: iam - login history, own sessions, profile, admin login-log search

- **Number:** 0007
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R15, R22, R24-R26, R35 (own), R41, R48 (label) ; AC5, AC6, AC10, AC15 ; depends on Plans 0004, 0006
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

Every user can see their own login history and active sessions (and sign out any device) and edit their own profile; a Super Admin can search and export the whole login log. The log is the record required by the owner: who, when, from which browser/OS/device/IP, and when and why they logged out.

## 2. Out of scope

Login-log writing (done in 0004), retention/archiving (none by decision D10), new-device alert email, GeoIP, the system-account label logic beyond calling the resolver (0009 supplies it; until then the label resolver returns the real user).

## 3. Approach

**Api:**
- Queries with projection DTOs (no entities out): own `GET auth/login-history` (paged, newest first), `GET auth/sessions` (active, with device/browser/IP, `last_seen_at`, marks the current one), `DELETE auth/sessions/{id}` (own only; `USER_LOGOUT`-style close with reason `REVOKED_BY_USER`, added to R24's list), `PUT auth/profile` (name; email/mobile change requires `currentPassword`, uniqueness -> 409).
- Admin `GET login-logs` (filters user, role, result, IP, date range; sort; paged) and `GET login-logs/export` (CSV streamed, same filters, capped rows per request, fields as R22, formulas escaped against CSV injection). `[Authorize(Roles = "SUPER_ADMIN")]`.
- `IActorLabelResolver` port used when projecting rows for admins: returns the real user, or the fixed label "System" for the hidden account (R48; implementation completed in 0009).
- Query performance checked against the indexes from spec §4 with a realistic volume (generated rows in the scratch database, `EXPLAIN` recorded).

**Ui:**
- `admin` and `member`: "My profile" page (edit name; change email/mobile with password confirmation), "My sessions" list with sign-out per device and "sign out everywhere", "Login history" table (time, result, browser, OS, device, IP, logged-out time and reason, session duration derived); last-login info in the header user dropdown.
- `admin` only: "Login log" page under user management with the filters and an Export button; the user detail page (0006) gets a login-log tab.
- Texts and reason labels from the catalogue.

## 4. Tasks

- [x] 1. Own login-history and sessions endpoints, profile update, `REVOKED_BY_USER` reason
- [x] 2. Admin login-log search + CSV export, `IActorLabelResolver` port
- [x] 3. Index/`EXPLAIN` check with generated volume
- [x] 4. Ui: profile, sessions, history pages; admin log page + export; dropdown info; catalogue
- [x] 5. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27 against throw-away databases (created, migrated, dropped by the scripts), the API started from a separate build folder with `Auth:Password:Iterations=1000` (Plan 0012) so the runs are fast. Build 0 warnings, 0 errors; `eslint` on the changed UI files: 0 errors (18 warnings, all in legacy files); the three UI apps built in the staging configuration. Scripts `api-check8.sh`, `ui-real4.sh`, `run7.mjs`, `explain.sql` are not committed.

**Migrations** (SQL shown before applying, applied to the local `book_inventory`, target confirmed): `Iam_RevokedByUser` (replaces the two `logout_reason` / `revoke_reason` CHECK lists to include `REVOKED_BY_USER`) and `Iam_LoginLogTimeIndex` (`CREATE INDEX ix_user_login_log_login_at ON iam.user_login_log (login_at)`).

**EXPLAIN with 300,000 generated login rows and 2,000 users** (`explain (analyze)`, PostgreSQL 17):

| Query | Plan and time |
| --- | --- |
| own history, newest 20 of one user | `Index Scan Backward using ix_user_login_log_user_id_login_at`, 0.11 ms; count of that user 0.69 ms |
| admin: one IP, last 7 days | `Index Scan Backward using ix_user_login_log_ip_address_login_at`, 0.11 ms |
| admin: result `FAILED_LOCKED`, 1-day range | `Index Scan Backward using ix_user_login_log_result_login_at`, 0.09 ms |
| admin: one user, 30 days | `ix_user_login_log_user_id_login_at`, 0.12 ms |
| admin: no filter, newest 20 | **before:** parallel sequential scan + top-N sort; **after** the new index: `Index Scan Backward using ix_user_login_log_login_at`, 0.06 ms |
| admin: date range only (1 day) count | **before:** parallel sequential scan, 55 ms; **after:** `Index Scan using ix_user_login_log_login_at`, 0.97 ms |

| Rule / AC | Input | Observed |
| --- | --- | --- |
| R12 | a must-change token on `auth/sessions`, `auth/login-history`, `PUT auth/profile` | `403 iam.auth.password_change_required` on all three |
| R15, AC6 | a user with two sessions (Windows Chrome, Android Chrome) lists sessions; ends the other one; ends somebody else's; ends an unknown id | 2 sessions with browser, OS, device, exactly one `isCurrent`; `204`, the ended token `401`, the caller's token `200`; session and log row both `REVOKED_BY_USER`; `404 session_not_found` for another user's id and for an unknown id |
| R26 | own `login-history`, `pageSize=2` | only the caller's 3 rows (none of another user's), newest first with browser/OS, 2 rows and `totalPages 2` |
| R35, R41, AC15 | own profile: name only; e-mail without / with a wrong password; a taken e-mail with the right password; the same e-mail again; a new e-mail with the password | `204`; `400 current_incorrect` x2; `409 duplicate_email`; `204` (unchanged needs no password); `204` and the stored e-mail changed |
| R26, AC10 | a `SALES_CRM` token on `login-logs` and `login-logs/export` | `403` and `403` |
| R26 | search: no filter; `userId`; `role`; `result`; `ipAddress` (`::1`, `9.9.9.9`); dates; `orderBy` outside the whitelist and `Result ASC`; `pageSize=500` | totals 4, 3, 3, 1, 4 and 0; future and past ranges 0; an unknown sort column answers `200` newest first (it cannot probe columns); `Result ASC` orders by result; the page size is capped to 200; a bad IP `400 iam.login_log.invalid_ip`; a bad role `400` |
| R48 | a hidden system account with a log row | the row shows `actor: {userId: null, userName: null, isSystem: true}`, no user name and no id anywhere in the body; filtering by its id still finds it, still labelled; the export writes `System` |
| R26 | CSV export | `200 text/csv`, `Content-Disposition: attachment`, `Cache-Control: no-store`, the fifteen R22 columns; a login with identifier `=cmd|calc` and user agent `=1+1` is written as `'=***c` and `'=1+1` (no cell starts with a formula character); enum values use the wire spelling (`FAILED_UNKNOWN_USER`) |
| R26 | 10,100 rows, export everything; then narrowed to one user | `400 iam.login_log.export_too_large`; `200` |
| UI (Edge, same origin, real API) | the user dropdown; the profile page; a second device (Android user agent, one wrong password first); sessions; history; the login log; export; the user detail | the dropdown offers My profile, My sessions, Login history and the sign-in time; the profile page hides the password field until the e-mail changes, a save without the password shows the catalogue text of `iam.password.current_incorrect`, with it the e-mail is saved; the sessions page lists this device and the Android device, signing out the other leaves 1 and the other device is at `/login` on its next request; the history shows the failed attempt, the browser/OS per row and "Ended from another device"; the login-log page lists the whole log, the result filter narrows it to 1 row, Export CSV downloads a `text/csv` file; the user detail page has a login-log section |

**Defects found while building it and fixed:** the login-log search projected a nullable joined entity, which EF cannot translate (now column projection); the enum query binder claimed every nullable type (`Guid?`, `DateTime?`) and threw (now only enums); there was no index on `login_at` for the unfiltered and date-only searches (added, above); the admin profile route still pointed at the legacy page (now the shared profile screen).

**NOT verified:** the member app's three screens in a browser (the same shared components; only built and linted - no agent user exists in the check database); the user dropdown's "signed in" time is the session's login time, not the previous login; the "sign out everywhere" button in a browser (its API was verified in Plan 0004); an export at exactly the cap boundary.

## 6. Risks and notes


- Export size: capped per request with a clear message; large exports are a future concern.
- Own-session revoke adds the reason `REVOKED_BY_USER` to the spec's list (spec updated when this plan is closed).
