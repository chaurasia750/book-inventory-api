# Plan 0013: iam - hardening (backlog items B13, B21, B25, B31, B36)

- **Number:** 0013
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R1 (mobile form), R5 (last Super Admin), R12 (forced-change filter), section 5 (user list sort) ; AC15, AC20
- **Status:** Done
- **Created:** 2026-09-27   **Completed:** 2026-09-27

## 1. Goal

Five known gaps of Plans 0003-0007 are closed: two requests can no longer both pass the last-Super-Admin check, a duplicate account under a race answers 409 instead of 500, the user list can only be sorted by a whitelist, the forced-change filter matches whole paths, and a mobile number has one stored form whatever the caller typed.

## 2. Out of scope

BuildingBlocks items (B4 nested options, B6 problem factory, B40 CORS), the worker loop (B29) and every item that needs the owner's decision (B5, B10, B34, B37) or a real environment (SMTP, IIS, proxy).

## 3. Approach

- **B36 mobile (owner decision 2026-09-27: India, strip the prefix):** `IdentityNormalizer.NormalizeMobile` keeps digits only (as now), then removes a leading `91` when 12 digits remain, or a leading `0` when 11 remain, so `+91 98765 00001`, `098765 00001` and `98765 00001` are all `9876500001`. The length rule (10-15) is unchanged for other shapes. One place, so the aggregate, the uniqueness lookup and the registration all agree. **Data:** rows written earlier keep their old stored form, so a migration `Iam_MobileNormalised` rewrites stored 12/11-digit values to the 10-digit form; it can collide with an existing 10-digit twin, so the migration first reports such pairs (SQL shown before applying, applied only to the local `book_inventory`, target confirmed). Needs approval as a data rewrite.
- **B13 last Super Admin:** the check "at least one other visible active Super Admin" and the write that removes the last one run under one transaction-scoped PostgreSQL advisory lock (`pg_advisory_xact_lock`, a fixed key) taken by the store method that loads the account for a role/status change, so two concurrent requests serialise and the second sees the first's result. No schema change.
- **B25 duplicate under a race:** **changed during the work:** the backlog said the race answers 500; it does not. The platform `GlobalExceptionHandler` already maps a unique violation to `409` (generic text, no field code). Nothing was changed; giving the race the same `iam.user.duplicate_*` code as the lookup would need a BuildingBlocks change (Backlog B43).
- **B21 user list sort:** `UserDirectory.SearchAsync` sorts only by a whitelist (`UserName`, `FullName`, `Email`, `Role`, `Status`, `CreatedAt`, `LastLoginAt`), like the login-log search; any other name falls back to the default order and cannot probe a column.
- **B31 forced-change filter:** the allow-list compares the full route (`api/v1/auth/me`, `logout`, `change-password`, `logout-all`) instead of a path suffix.

## 4. Tasks

- [x] 1. Domain: `NormalizeMobile` prefix rule + message text for the 409 unchanged
- [x] 2. Infrastructure: migration `Iam_MobileNormalised` (SQL shown first, collision report, approval)
- [x] 3. Application/Infrastructure: advisory lock in the role/status change path (B13)
- [x] 4. Race-safe duplicate answer (B25): checked, no code change needed (the platform handler already answers 409; see Verification)
- [x] 5. Application: whitelist sort in `UserDirectory` (B21)
- [x] 6. Api: exact-path allow-list in `MustChangePasswordFilter` (B31)
- [x] 7. Verify by hand (section 5); spec R1/R5/R12 text, plan status, backlog rows updated

## 5. Verification

Run on 2026-09-27 against a throw-away database (created, migrated, dropped by the script `api-check12.sh`, not committed), the API from a separate Release build with `Auth:Password:Iterations=1000`. Build 0 warnings, 0 errors.

**Migration `Iam_MobileNormalised`** (data only; SQL shown before applying). Legacy rows inserted at the previous migration, then migrated:

| Before | After |
| --- | --- |
| `919876500001` (12 digits, prefix 91) | `9876500001` |
| `09876500002` (11 digits, prefix 0) | `9876500002` |
| `919876500003` and `9876500003` (the 10-digit twin exists) | both **unchanged** (the rewrite would break the unique index; reported, not forced) |
| `9876500004`, `919876500005123` (15 digits) | unchanged |

Applied to the local `book_inventory` too (target `Database=book_inventory` confirmed); it holds 3 users and none has a prefixed mobile, so it changed nothing there.

| Item | Input | Observed |
| --- | --- | --- |
| B36, R1 | staff creation with `+91 98765 00001`, `098765 00002`, `9876500001`, and the number of the un-rewritten twin | each `409 iam.user.duplicate_mobile` |
| B36 | a new number typed `+91-98765-00099`; then the same number plain | `201`; then `409 duplicate_mobile` (one stored form) |
| B36 | `+91 12345` | `400` (too short after stripping) |
| B31, R12 | a must-change token: `auth/me`, `users`, `auth/sessions`, `auth/change-password`, then `auth/logout-all` with the new token | `200`; `403 iam.auth.password_change_required`; `403 ...`; `200`; `204` |
| B21 | `users?orderBy=PasswordHash`, `=Bogus`, `=RowVersion`, `=FullName ASC`, `=FullName DESC` | `200` in the default order for the three that are not whitelisted; ordered by name ascending and descending for the two that are |
| B13, R5 | two Super Admins deactivate each other in the same instant, 6 rounds (two parallel requests, each with its own token) | every round exactly one `204` and one `409 iam.user.last_super_admin`; one Super Admin stayed active in all 6 rounds, never zero |
| B25 | the same e-mail created by two parallel requests, 6 rounds | one `201` and one `409` per round, never `500`; in the round where the unique index (not the lookup) refused, the answer was `409` without a field code (the platform's generic text) |

**NOT verified:** that B13 fails *without* the lock (the race was not reproduced on the old code, so 6 clean rounds are consistent with the fix but do not prove the old failure); a look-alike path such as `/x/auth/me` for B31 (no such endpoint exists; the exact-route regex is what prevents it); the mobile rule for non-Indian numbers (out of scope, owner decision).

## 6. Risks and notes

- The mobile rule is India-specific by owner decision; a later international rollout needs a country-aware value object (new plan).
- The advisory lock is per database; it serialises the rare admin action, not user traffic.
