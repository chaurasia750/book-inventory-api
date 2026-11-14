# Plan 0004: iam - authentication (login, sessions, logout) end to end

- **Number:** 0004
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R7-R15, R20-R24, R27 ; AC2-AC7 ; depends on Plans 0001-0003 (API) and 0002 (UI)
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-26

## 1. Goal

A user can log in, stay logged in with a rotating refresh cookie, and log out; every attempt (success or failure) is in the login log with browser/OS/device/IP and logout details; lockout, rate limiting and forced password change are enforced by the server. The `public-site` login page works against it and lands each role in the right app.

## 2. Out of scope

Password change/forgot/reset (0005), account management (0006), login-history/sessions/profile endpoints and admin log search (0007), registration hand-off (0008), system-account rules (0009).

## 3. Approach

**Api - Application (use cases):** `Login`, `Refresh`, `Logout`, `LogoutAll`, `GetMe`. Login is a result-returning use case (no exceptions for expected failures) so its failure path can be committed by `[PersistOnFailure]`.
- Password verification with the `IPasswordHasher` port and the PBKDF2-HMAC-SHA512 implementation built in Plan 0003 (R16, D19, `$pbkdf2-sha512$` self-describing format). No package: the BCL `Rfc2898DeriveBytes` is enough, and Plan 0003 already added the verifier and a rehash-needed answer.
- Lockout and counters per `AuthOptions` (R9); lazy suspension lift at login (R32); status checks and generic vs specific errors (R8).
- Access token: JWT with `sub`, `role`, `sid`, `name`, `pwd_change`, `amr = pwd`, lifetime from options. Refresh token: 256-bit random, only SHA-256 stored in `UserSession`, rotated on every refresh, reuse detection revokes the session and logs `REFRESH_REUSE` (R13).
- `LoginLog` written for every attempt with the fields of R22; the closing of a log row on logout/revoke/expiry with the reasons of R24.

**Api - Infrastructure / Api:**
- `UAParser` package (approved) behind an `IClientInfoParser` port; real client IP via `ForwardedHeaders` limited to configured trusted proxies (R23).
- Built-in `AddRateLimiter` policies for `login` (per IP, from options) (R10).
- JWT bearer `OnTokenValidated`: the `sid` session must still be active (short in-memory cache invalidated on revoke) so a revoked or suspended user fails on the next request (R31 groundwork); `must_change_password` enforced by an authorization filter that allows only change-password, me and logout (R12).
- Refresh cookie: HttpOnly, Secure, `SameSite=Strict`, path scoped to `/api/v1/auth`, names/lifetimes from options.
- Hosted service that closes expired sessions with `EXPIRED` (R24).
- Controllers `AuthController`: `POST auth/login`, `auth/refresh`, `auth/logout`, `auth/logout-all`, `GET auth/me`; login action marked `[PersistOnFailure]`.
- Message codes for all outcomes in `IamMessages`.

**Ui (`public-site`, guards, layout):**
- Login page (existing component adapted): identifier + password + "remember me", error text from the catalogue by `code` (invalid credentials, locked with retry time, pending, suspended, inactive, rejected), disabled state while submitting, `returnUrl` validation (same origin, only `/admin`/`/member`).
- `AuthService` wired to the real endpoints; refresh bootstrap at app start in all three apps; role -> app redirect (staff `/admin`, `MEMBER` `/member`); logout in the user dropdown (calls API, clears memory, hard redirect to `/login`); `mustChangePasswordGuard` redirects to the forced-change route created in 0005.

## 4. Tasks

- [x] 1. Application: options usage, `Login`/`Refresh`/`Logout`/`LogoutAll`/`GetMe` use cases + result types
- [x] 2. Token service (JWT + refresh token + hashing), session rotation and reuse detection
- [x] 3. Login log writing and closing; `IClientInfoParser` (UAParser); forwarded headers; rate limiter
- [x] 4. JWT bearer session check, `must_change_password` filter, cookie handling, expiry sweeper
- [x] 5. `AuthController` + `[PersistOnFailure]` on login; message codes
- [x] 6. Ui: login page, `AuthService`/interceptor wiring, bootstrap, guards, logout, role redirect, catalogue entries
- [x] 7. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-26 against a throw-away database `iam_api_check` (created, migrated with `dotnet ef database update`, dropped by the script), the API started from a separate build folder over HTTPS (`https://localhost:7199`), seeded Super Admin from user-secrets, `Auth:RateLimit:LoginPermitPerMinute` raised only where a check needed it. Scratch scripts (`api-check2.sh`, `api-check3.sh`, `ui-real.sh`) live in the session scratchpad and are not committed. Build: `dotnet build src/BookInventory.Api` 0 warnings, 0 errors (the normal solution build was blocked by a running dev API holding the DLLs; the same projects built to another output folder).

| Rule / AC | Input | Observed |
| --- | --- | --- |
| AC2, R8 | unknown user vs wrong password | both `401 {"code":"iam.auth.invalid_credentials"}`, identical bodies; response time 0.099 s (unknown) vs 0.105 s (wrong password) after the dummy verification was added |
| R7, R11 | login by e-mail (staff) | 200, body `accessToken`, `accessTokenExpiresAt`, `refreshTokenExpiresAt`, `user{..., role:"SUPER_ADMIN", mustChangePassword}`; no refresh token in the body; cookie `book_inventory_refresh; secure; samesite=strict; httponly; path=/api/v1/auth` |
| R13, AC7 | refresh with the cookie; replay the previous cookie; then the newest cookie | 200 with a new `user`; replay `401 iam.session.refresh_reuse`; newest cookie `401 iam.session.revoked`; old access token `401`; session row `REFRESH_REUSE`, login-log row closed `REFRESH_REUSE` (the revoke survived the 401) |
| R14, AC6 | logout, then use the token; logout-all with two sessions | `204`, cookie cleared, token `401`; row `USER_LOGOUT`; logout-all ends both, both tokens `401`, rows `LOGOUT_ALL` |
| R9, AC3, AC4 | 5 wrong passwords, then the right one | `401` x5 (generic), then `423 iam.auth.account_locked`; `failed_login_count = 5`, `locked_until` set; `FAILED_BAD_PASSWORD` x5 and `FAILED_LOCKED` rows exist although every answer was 401/423 |
| R8 | right password with status PENDING / INACTIVE / SUSPENDED / REJECTED; wrong password with the same statuses | right: `iam.auth.pending_approval`, `account_inactive`, `account_suspended`, `application_rejected`; wrong: always the generic code |
| R32 | suspension whose end date passed, right password | 200, status `ACTIVE` |
| R11 | login with `rememberMe`, then refresh | cookie expiry +30 days before and after the rotation; `is_remember_me = t`, ttl > 29 days |
| R24 | session expired in the database, then refresh | `401 iam.session.expired`; session revoked at `expires_at` with `EXPIRED`; login-log row `EXPIRED` |
| R10 | 16 attempts from one address, limit 15 | 429 `iam.auth.rate_limited` with `Retry-After: 60` |
| R22, R23, AC5 | `X-Forwarded-For: 8.8.8.8` from an untrusted address; iPhone, iPad, Mac Safari, Android Chrome, Googlebot, Windows Chrome user agents | IP stored `::1` (socket), not 8.8.8.8; device types `MOBILE`, `TABLET`, `DESKTOP`, `MOBILE` (Chrome Mobile, Android), `BOT`, `DESKTOP`; browser/OS parsed; unknown identifiers masked (`n***e`) |
| UI (Task 6) | Edge against the built apps, same origin, `/api/v1` proxied to the real API | wrong password shows the catalogue text inline; e-mail login lands in `/admin` and shows the real user and "Super Admin"; only `rrj.session` in storage and no cookie readable; reload restores the session through `/auth/refresh`; sign out returns to `/login` and a deep link does not try a refresh; the locked account shows the `iam.auth.account_locked` text (423) |

**Defects found and fixed in this plan (review + verification):** forged `X-Forwarded-For` was believed (the framework trusts every caller when both proxy lists are empty, so the middleware now only runs when a proxy is configured); every phone was logged as `DESKTOP`; `Retry-After` was missing; the session-ending steps existed in five copies (now `SessionEndService`); the refresh cookie ignored its lifetime setting and the session and cookie expiries came from two clocks; a revoked session made refresh throw; unknown users were answerable by response time; the remember-me flag was not stored on the session, so a rotation dropped a 30-day session to 12 hours; login/refresh/me returned three different user shapes and the role as `SuperAdmin` (the platform JSON enum converter now writes UPPER_SNAKE, a one-line change in `library/BuildingBlocks/Configuration/Extensions/FilterExtension.cs`); duplicated seeder/directory Super Admin query, password policy, cookie options and text rules; dead code from Plan 0003 removed (`CanLoginAt`, `Touch`, `AsSource`, `IsGranted`).

**NOT verified:** `must_change_password` filter on a non-auth endpoint (there is none yet; only `me`/logout are exercised while the flag is set); the expired-session sweeper's 5-minute timer (its logic is the same `SessionEndService` path the lazy expiry check above exercises); several instances or a real reverse proxy with `ReverseProxy:KnownProxies` set; `refresh` from two tabs at the same instant (a second use of a just-rotated token revokes the session by design, R13); the sliding session lifetime has no absolute cap (spec is silent - decision for the owner).

## 6. Risks and notes

- Failed logins must survive the 4xx: the most important thing to see in the recorded outputs.
- Per-request session check adds a lookup; the short cache keeps it cheap and is dropped on revoke (single-instance deployment assumed).
