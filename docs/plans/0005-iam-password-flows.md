# Plan 0005: iam - password flows (change, forced change, forgot, reset)

- **Number:** 0005
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R12, R16-R19, R56, R57 (generator) ; AC1, AC8 ; also [docs/specs/core.md](../specs/core.md) (N1, N2, N9, N13) ; depends on Plan 0004 and **Plan 0011** (notification outbox in `core`, must be Done first)
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

Users can change their password, are forced to change it when required, and can recover an account through an emailed reset link. The reset mail is not sent by iam: it is recorded as a notification in the `core` outbox (Plan 0011) and sent later by its worker.

## 2. Out of scope

The outbox, worker, SMTP/logging senders and the master layout (Plan 0011), choosing the production provider (core CO1), staff creation (0006, which uses the temp-password generator built here), account edit screens.

## 3. Approach

**Api:**
- No email code in iam. `ForgotPassword` calls `INotificationService.QueueAsync` from `core` with template code `PASSWORD_RESET`, the recipient, variables `fullName`, `resetLink`, `expiresInMinutes` and the idempotency key `password-reset:<token id>`, **inside the same transaction** as the new `PasswordResetToken` (R56, core N1/N2). iam contributes the default `PASSWORD_RESET` template (HTML body, `is_sensitive = true`, wording from the owner-reviewed text) through core's `INotificationTemplateSource` (core N13). In development the logging sender shows the link in the log (core N12).
- `PasswordPolicy` validator from `Auth:Password:*` (R16); `TemporaryPasswordGenerator` (cryptographically random, meets the policy) for R57.
- Use cases: `ChangePassword` (current password required, must differ, revokes the user's other sessions with `PASSWORD_CHANGED`, clears `must_change_password`, returns a fresh access token) (R19); `ForgotPassword` (always 202 with the same body; only `ACTIVE` users get a mail; 256-bit token, SHA-256 stored, lifetime from options, older unused tokens invalidated; per-IP and per-identifier rate limit) (R17); `VerifyResetToken` (validity only); `ResetPassword` (sets password, clears the flag, resets lockout, marks token used, revokes all sessions with `PASSWORD_RESET`) (R18).
- Controller endpoints `POST auth/change-password`, `auth/forgot-password`, `GET auth/reset-password/verify`, `POST auth/reset-password`; `change-password` allowed while `must_change_password` (R12).
- Reset link points to the UI route `/auth/reset-password?token=` (base URL from options).
- All messages as codes in `IamMessages`.

**Ui:**
- `public-site`: forgot-password and reset-password pages adapted to the new endpoints (token verification on load, strength hints from `strongPasswordValidator` aligned with `Auth:Password`, confirm-match); the login page's forgot link goes to the page (the legacy modal is removed); neutral confirmation text that never says whether the email exists.
- `admin`/`member`: change-password page (existing `ui` component adapted) plus the **forced change** screen (route used by `mustChangePasswordGuard`; blocks the rest of the app until done, then continues to the requested page); user dropdown link.
- Catalogue entries for all outcomes.

## 4. Tasks

- [x] 0. Precondition: Plan 0011 is Done (`INotificationService` available)
- [x] 1. `PASSWORD_RESET` template source, password policy, temporary password generator
- [x] 2. Use cases + token handling for change/forgot/verify/reset, session revocation
- [x] 3. Controller endpoints, rate limits, message codes
- [x] 4. Ui: forgot/reset pages, change-password page, forced-change screen, guard wiring, catalogue
- [x] 5. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27 against a throw-away database `iam_api_check` (migrated, dropped by the script), the API started from a separate build folder over HTTPS with the logging sender (`Email:Host` empty, `Email:LogBody` on in Development), `Auth:Reset:LinkUrl` pointed at the test origin, rate limits raised only where a check needed it. Build: `dotnet build src/BookInventory.Api` 0 warnings, 0 errors; `eslint` on the changed UI files clean; the three UI apps built in the staging configuration (same origin, `--base-href` `/`, `/admin/`, `/member/`). Scratch scripts `api-check6.sh`, `ui-real2.sh`, `run4.mjs` are not committed.

| Rule / AC | Input | Observed |
| --- | --- | --- |
| AC1 (second half), R12 | log in with the seeded account (`must_change_password = true`), change the password | token carries `pwd_change: true`; `change-password` answers 200 with a fresh access token whose `pwd_change` is `false`; the new token works on `me` |
| R19 | two sessions A and B; A changes the password | session B's token `401`; A continues; `user_session` holds one row `PASSWORD_CHANGED`; the old password no longer logs in, the new one does |
| R19, R16 | wrong current password; same password; `Sh0rt`, `password1234`, `PASSWORD1234`, `NoDigitsHere` | `400` with `iam.password.current_incorrect`, `iam.password.same_as_current`, `too_short`, `missing_uppercase`, `missing_lowercase`, `missing_digit` |
| AC8, R17 | forgot password for the known and an unknown address | both `202` with an empty body, byte-identical; one `core.notification` row (`PASSWORD_RESET`, recipient the known address) and one `password_reset_token`; the unknown address appears nowhere (log, tables); after the worker sent it the row is `SENT` with variables `[redacted]`; the Development log shows the link only for the known address |
| R17 | verify a good token, garbage, no token | `{"valid":true}`, `{"valid":false}`, `{"valid":false}` |
| R18 | locked account (4 failures, `locked_until` set, `must_change_password`), reset with a weak password, then a good one | weak: `400 iam.password.too_short`; good: `204`; `failed_login_count = 0`, unlocked, `must_change_password = false` |
| AC8, R18 | use the link a second time; verify it again | `400 iam.token.already_used`; `{"valid":false}` |
| AC8, R18 | old session cookie and its access token after the reset | refresh `401 iam.session.revoked`, token `401`; session rows `PASSWORD_CHANGED` x1, `PASSWORD_RESET` x2; the reset password logs in, the previous one does not |
| AC8 | token expired in the database | verify `{"valid":false}`; reset `400 iam.token.expired` |
| R17 | six forgot requests for one address in a row | five tokens in the hour, only one still unused; a sixth is silently not queued (still `202`) |
| R10 | 12 forgot requests a minute allowed, more sent | `429` with `Retry-After: 60` |
| UI, forced change | Edge against the built apps (same origin, real API): log in with the seeded account | redirected to `/admin/change-password` with the notice; another route bounces back; wrong current password shows the catalogue text; a weak or mismatching password is refused in the form; a valid change continues to `/admin/dashboard` without a new login and the flag is gone |
| UI, forgot/reset | the login page link, forgot for known and unknown, the emailed link, reset, the link again, login | the forgot link replaces the old modal; identical neutral text for both; the link from the Development log opens the reset form (token verified on load); success message; the link now shows as no longer valid; login with the reset password works |

**Defects found while building it:** the `ITokenService` names said "refresh" although the reset token uses the same opaque token (renamed `IssueOpaqueToken`/`HashOpaqueToken`); the access token claims were built in three places (now `IssueAccessTokenFor`, one place); the change-password screen posted `oldPassword` to the legacy API (now the shared `ChangePasswordScreenComponent` on `AuthService`).

**NOT verified:** the forced-change filter (R12) on a *non-auth* endpoint - iam has no other authenticated endpoint yet (only `me` and logout were exercised while the flag was set); a real SMTP server (Plan 0011 / core CO1); `TemporaryPasswordGenerator` is built for Plan 0006 and only compiled here - its use is verified there; per-identifier throttling is the per-hour token limit (a per-IP limit exists as well), not a separate counter.

## 6. Risks and notes

- Reset emails go to unverified addresses (no verification in v1, owner decision D5); that is an accepted limitation, recorded in the spec.
- Until a provider exists (core CO1) nobody receives real email; the logging sender is for development only and must not be the production wiring: the release checklist (Plan 0010) lists it.
- A queued mail is sent by the worker within its interval (default 30 s), not inside the request; the forgot-password answer therefore never depends on the mail server (and cannot reveal whether the address exists).
