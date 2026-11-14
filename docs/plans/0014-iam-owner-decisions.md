# Plan 0014: iam - owner decisions of the backlog review (absolute session life, hidden account 404, approve without forced change)

- **Number:** 0014
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R3, R13, R42, R43, R54 ; AC18
- **Status:** Done
- **Created:** 2026-09-27   **Completed:** 2026-09-27

## 1. Goal

Three decisions the owner took in the backlog review (B10, B34, B37) are in force: a session ends for good 30 days after the login however often it is refreshed, every management command against the hidden system account's id answers the same 404 as an unknown id, and approving an agent no longer forces a password change.

## 2. Out of scope

The platform decisions of the same review (Plan 0015), and anything that still needs a real environment.

## 3. Approach

- **B10 (owner: cap 30 days for everyone):** `Auth:Tokens:AbsoluteSessionDays` (default 30, 1-365, must not be shorter than `RememberMeDays`, checked at startup). `TokenSettings.AbsoluteEnd(session)` = the session's creation + the cap. A refresh slides the expiry but never past it, and a refresh after it closes the session as `EXPIRED` (`iam.session.expired`). No migration: the session's creation time already exists.
- **B34 (owner: all 404):** `IAccountStore.FindForManagementAsync` no longer looks past the hidden-account filter, so every command finds nothing and answers `iam.user.not_found`. The aggregate's own guard (403) stays as a second line of defence.
- **B37 (owner: no forced change on approve):** `AppUser.Approve` no longer sets `must_change_password`; the agent chose the password at registration.

## 4. Tasks

- [x] 1. `AbsoluteSessionDays` option, settings, rotation cap and expiry in the refresh use case
- [x] 2. `FindForManagementAsync` excludes the hidden account; docs of the store updated
- [x] 3. `Approve` without forced change
- [x] 4. Verify by hand (section 5); spec R3/R13/R42/R43/R54/AC18, backlog

## 5. Verification

Run on 2026-09-27 against a throw-away database (`api-check13.sh`, not committed), API from a separate Release build with `Auth:Password:Iterations=1000`. Build 0 warnings, 0 errors. No migration.

| Item | Input | Observed |
| --- | --- | --- |
| B37 | a pending account is approved; then deactivated and activated | `204`, `ACTIVE`, `must_change_password = false`; deactivate/activate `204` `204`, unchanged flag |
| B34 | all 15 calls of the user API (get, status-history, sessions, put, approve, activate, deactivate, suspend, reject, change-role, unlock, force-logout, force-password-change, reset-password, release-identity) against the system account id | all `404 iam.user.not_found`, identical to an unknown id; the account is unchanged (`ACTIVE`, `SUPER_ADMIN`, `System`) |
| B10 | `AbsoluteSessionDays=1`, `RememberMeDays=1`: remember-me login | the session is 24 h long |
| B10 | refresh inside the cap | `200` |
| B10 | the session's creation set to 23 h ago, refresh | `200`; the new expiry is at most one hour from now (`t`), not another 24 h |
| B10 | creation set to 25 h ago while `expires_at` is still in the future, refresh | `401 iam.session.expired`; the session is closed `EXPIRED` and its login-log row too |
| R54 | `AbsoluteSessionDays=10` with the default `RememberMeDays=30` | the host does not start: "Auth:Tokens:AbsoluteSessionDays must not be shorter than Auth:Tokens:RememberMeDays." |
| default | remember-me login with the defaults | the session is 30 days long |

**NOT verified:** the cap over a real 30 days (the creation time was moved in the database); sessions opened before this change are capped by their creation time too (not exercised).

## 6. Risks and notes

- A remember-me session is now a fixed 30 days from the login, no longer sliding: an agent who used the app daily for months had to log in only once before; now once a month.
