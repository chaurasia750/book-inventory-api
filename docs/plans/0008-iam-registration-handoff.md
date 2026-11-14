# Plan 0008: iam - agent registration hand-off and registration mode

- **Number:** 0008
- **Module:** `iam` (primary) ; also touches: `mlm` (future caller)
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R1, R3, R53, R55 (welcome text), R58 ; AC25 ; depends on Plans 0004, 0006
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

`iam` offers the capability the future `mlm` agent-registration use case will call, and the `Registration:Mode` switch behaves as decided: approval required (default; generic welcome message) or instant login (account active, session issued, straight to the dashboard).

## 2. Out of scope

The registration form, its SRS 3.1 fields, sponsor code, KYC/bank and the member-code generator (all `mlm`, open item O3). This plan builds only what `iam` owns and proves it with a stand-in caller.

## 3. Approach

**Api:**
- Application capability `IMemberUserRegistration.Register(userId, fullName, email, mobile, password)`: the caller (mlm) supplies the generated user id; `iam` checks uniqueness (R1) including identities reserved by a rejected application (R58), creates the `MEMBER` account, and according to `Registration:Mode` either leaves it `PENDING` (no session, result `PendingApproval`) or creates it `ACTIVE` and issues the same access/refresh tokens as a login (result `SignedIn`, no forced password change). Writes a login-log row for the instant sign-in.
- `RegistrationOptions` (`Mode`) validated at startup (R54); no other switch.
- Events `UserActivated`/`UserRejected` (already raised by 0006) are documented as the extension points for `mlm`.
- No public endpoint is added here: registration is exposed by `mlm`. For verification a scratch caller stands in.

**Ui:**
- Shared, reusable pieces in the `auth` lib for the future signup page: a `RegistrationResult` handler that, on `PendingApproval`, navigates to a welcome page showing the catalogue text ("thank you, please contact the admin team"), and on `SignedIn` stores the returned access token in memory and hard-redirects to the member dashboard.
- Welcome page (public-site route `/welcome`) with text from the catalogue only (R55).
- The legacy signup page stays removed until the `mlm` registration plan.

## 4. Tasks

- [x] 1. `IMemberUserRegistration` implementation with both modes, reserved-identity check
- [x] 2. `RegistrationOptions` + validation; message codes (welcome text, duplicate messages)
- [x] 3. Ui: result handler in `auth` lib, welcome page, catalogue
- [x] 4. Verify by hand with a scratch caller (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27 against a throw-away database, the API started from a separate Release build with `Auth:Password:Iterations=1000`. Build 0 warnings, 0 errors; `eslint` on the changed UI files: 0 errors; the public-site built in the staging configuration. **The stand-in caller** was a temporary `[AllowAnonymous]` probe controller that called `IMemberUserRegistration` and wrote the refresh cookie, plus a temporary Angular route that posted to it and handed the answer to `RegistrationHandoffService`; both were deleted again. Scripts `api-check10.sh`, `ui-real5.sh`, `run8.mjs` are not committed. No migration.

| Rule / AC | Input | Observed |
| --- | --- | --- |
| AC25, R53 (`ApprovalRequired`) | register `RJ100245` (email, mobile, a policy-conforming password) | `200 {"outcome":"PENDING_APPROVAL","messageCode":"iam.registration.pending_approval","signIn":null}`, no `Set-Cookie`; row `MEMBER`, `PENDING`, hashed password (`$pbkdf2-sha512$...`); no session and no login-log row; a login with the right password answers `401 iam.auth.pending_approval` |
| R1, R16 | same user id; same e-mail in upper case; same mobile written `98765-00001`; password `weak`; mobile `123`; empty e-mail | `409 iam.user.duplicate_username`, `409 ...duplicate_email`, `409 ...duplicate_mobile`, `400 iam.password.too_short`, `400 iam.user.invalid_mobile`, `400 iam.user.email_required`; still exactly one member row (each refusal rolled back) |
| R58, AC27 (second half) | a Super Admin rejects RJ100245; a new application with its e-mail, then with its mobile; `release-identity`; the same application again | `409 duplicate_email`, `409 duplicate_mobile`; `204`; `200 PENDING_APPROVAL` |
| AC25 (`InstantLogin`, only the configuration value changed, restart) | register `RJ100248` | `200 {"outcome":"SIGNED_IN","signIn":{accessToken,...,user}}`; `Set-Cookie: book_inventory_refresh=...; path=/api/v1/auth; secure; samesite=strict; httponly`; row `MEMBER`, `ACTIVE`, `must_change_password = f`; a `SUCCESS` login-log row with a session; token claims `role MEMBER`, `pwd_change false`; `auth/me` `200`; `auth/refresh` with the issued cookie `200`; a normal login as the agent afterwards `200` |
| R54 | `Registration__Mode=Bogus` | the host does not start: "Failed to convert configuration value 'Bogus' at 'Registration:Mode'" |
| UI, AC25 (Edge, same origin, real API) | `ApprovalRequired`: the stand-in page registers and hands the answer over | the browser is on `/welcome`, which shows "Thank you for registering ... contact the admin team" from the catalogue; no refresh cookie was set |
| UI, AC25 | `InstantLogin`: the same | the browser lands on `/member/overview` (member dashboard) and stays there (session restored through the cookie, no bounce to `/login`); the cookie is HttpOnly and SameSite=Strict |

**NOT verified:** a real registration form and the `mlm` endpoint (the legacy sign-up page still posts to the old `/members/registration`; the `mlm` registration plan replaces it), so the full path form -> API -> welcome/dashboard was only shown with the stand-in; rate limiting of the future registration endpoint (an `mlm` concern); the registration outcome when `mlm`'s own part of the transaction fails after the account was added (both roll back in one request transaction - not exercised, `mlm` does not exist yet).

## 6. Risks and notes

- Full end-to-end (a real signup form) can only be shown when the `mlm` registration plan exists; this plan is verified with the stand-in and that limitation is stated in the spec when closed.
