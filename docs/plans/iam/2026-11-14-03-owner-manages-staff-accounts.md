# Plan iam#03: The Owner creates and manages Staff accounts

- **File:** `docs/plans/iam/2026-11-14-03-owner-manages-staff-accounts.md`
- **Kind:** Backend
- **Spec:** [../../specs/iam.md](../../specs/iam.md) - rules covered: R65, R66, R67; AC36, AC37, AC38
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `iam#01`
- **UI plan:** iam#04
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/company-accounts` by the owner's explicit instruction)

## 1. In one minute
After this plan an Owner can create logins for their company's people (`childrenbookcenter.ramesh` style), see and manage only their own company's Staff, and reset a Staff password to a new temporary password shown once. The Super Admin can reset any Owner or Staff password the same way. Try it: log in as an Owner, `POST /api/v1/staff {fullName, shortName}`, then log in as the new Staff with the returned password.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Iam` (`CreateStaff` factory, temporary-password reset behaviour, reset record), `BookInventory.Application/Iam` (Staff use cases, query service scoped by `ICurrentCompany`), `BookInventory.Api/Controllers/Iam/StaffController.cs`, the `users` controller for the Super Admin reset. Also touches `BookInventory.Infrastructure/Persistence` (one new table, a wider user name column and the migration `Iam_StaffAccounts`), `IUserDirectory` and `UserSearchQuery` (a company filter) and `UsersController` (one Super Admin endpoint). Leaves out: what Staff may do in business modules (Q14, decided when those modules are built) and Staff screens (iam#04).

## 3. Approach
Every Staff query and command is scoped by the Owner's company from `ICurrentCompany`, never from input (R66); a record outside that scope answers `404`. The short name is validated (letters and digits, 2-20 characters) and joined to the Owner's user id; uniqueness is the existing R1 check. A reset sets a new generated password, `must_change_password`, revokes sessions through the existing revoker and writes an actor/time record. Schema change: yes, `iam.app_user.user_name` widened from 32 to 64 characters (a Staff id is the Owner's id up to 32 characters plus a dot plus a short name up to 20) and one new insert-only table `iam.user_password_reset` (`user_id`, `reset_by`, `reset_at`), written when a reset happens (R67). Because only one schema-changing plan per module may be open, this plan starts after iam#01 is `Done` (the `Depends on` line).

## 4. Tasks (at most 8)
- [x] 1. Domain: `AppUser.CreateStaff` (role `Staff`, company of the Owner, `must_change_password`), and a reset-to-temporary-password behaviour on the aggregate (R65, R67).
- [x] 2. Persistence: `UserPasswordReset` child entity, configuration and migration; show the SQL and the target database `book_inventory` to the user before applying (R67).
- [x] 3. Use case `CreateStaff` for an Owner: validates the short name, builds the user id, returns the one-time password (R65).
- [x] 4. Query service and use cases for the Owner: list, detail, sessions, deactivate, activate, unlock, force logout, reset password, all scoped to the Owner's company and to `Staff` (R66).
- [x] 5. `StaffController` (`OWNER` only) and `POST users/{id}/temporary-password` for the Super Admin (R66, R67).
- [x] 6. Messages for the new error codes in `IamMessages` and the catalogue.
- [x] 7. Build at 0 warnings, 0 errors; check size limits and `docs/Code_Map.md`.
- [x] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R65`, `AC36` | 1, 3, 5 | Owner of A: `POST staff {fullName: "Ramesh", shortName: "ramesh"}` -> 201, user id `<ownerid>.ramesh`, password once, `company_id` = A (SQL), first login forces a change; same short name again -> 409; `role: OWNER` in the body is ignored or rejected **Observed (2026-11-14, throw-away DB, 9 migrations applied by the app at start):** `POST staff {Ramesh Kumar, Ramesh}` -> 201, user id `childrenbookcenter.ramesh`, password once, `Cache-Control: no-store`; row `STAFF|ACTIVE|must_change=t|company = Owner's|no email|no mobile`; first login -> `mustChangePassword` true; same short name -> 409 `iam.user.duplicate_username`; a body with `role: OWNER` and another `companyId` -> still STAFF of the Owner's company; short names `r`, `ram esh`, 21 characters, `rám` -> 400 `iam.staff.invalid_short_name`. |
| `R66`, `AC37` | 4, 5 | Owner of A lists only A's Staff; `GET staff/{id}` of B's Staff, of A's Owner, of a Super Admin -> 404 identical to an unknown id; deactivate without a reason -> 400, with a reason -> 200 and the Staff's old token -> 401; unlock and force logout work **Observed:** Owner of A lists exactly A's two Staff; `companyId` and `role` in the query ignored; search by name works; `GET staff/{id}` of B's Staff, of A's Owner and of a Super Admin -> 404 `iam.user.not_found`, identical to an unknown id; sessions of B's Staff -> 404; deactivate without a reason -> 400 `iam.user.reason_required`, with a reason -> 204, the Staff's old token -> 401 and login -> `iam.auth.account_inactive`; deactivate B's Staff -> 404; activate -> 204 and login works; force-logout -> 204 and token -> 401; unlock -> 204; 2 rows in `iam.user_status_history`. |
| `R67`, `AC38` | 1, 2, 5 | Owner of A: `POST staff/{id}/temporary-password` -> new password once, old password and old sessions rejected, `must_change_password = true`, a record with actor and time; Owner of B -> 404; Super Admin: `POST users/{ownerId}/temporary-password` -> works **Observed:** Owner of A: `POST staff/{id}/temporary-password` -> 200 with a new password once (`no-store`); the old password -> 401, the old session -> 401; the new password logs in with `mustChangePassword` true; 1 row in `iam.user_password_reset` with the Owner as `reset_by`; Owner of B -> 404; Owner on own id via the Staff route -> 404; Super Admin: `POST users/{ownerId}/temporary-password` -> 200 and the Owner's old password -> 401; on a `SALES_CRM` account -> 400 `iam.user.password_reset_not_allowed`; on themselves -> 400 `iam.user.self_action_forbidden`; an Owner on the Super Admin route -> 403. |
| company `R14`, iam `R68`, `AC39` | 5, 8 | A Staff created through `POST staff` logs in; calling `companies`, `users`, `staff` -> 403; own profile and change-password -> success **Observed:** Staff token: `POST staff`, `GET staff`, `GET companies`, `GET users`, `POST staff/{id}/temporary-password` -> 403 each; `GET auth/me` -> 200; `GET companies/current` -> its own company (200). |

## 6. Not verified
- The migration `Iam_StaffAccounts` (widen `user_name` to 64, add `iam.user_password_reset`; additive) was applied only to throw-away databases. It was **not** applied to the local development database `book_inventory`; the app applies it at its next start. The SQL was shown to the owner and is waiting for confirmation.
- The login rate limit (10 a minute per address) made the first check run fail with 429; it was rerun with `Auth:RateLimit:LoginPermitPerMinute` raised for the check process only.
- A Staff account with an e-mail or mobile does not exist yet (the Owner form has no such field); the reset by mail (R17) for such accounts was not exercised.
- Two parallel creations of the same Staff id were not exercised (the unique index is the guard).

## 7. Risks and notes
The temporary password appears only in the response and is never logged. An Owner resetting their own password is not allowed through this route (the Owner uses change-password). Rollback of the migration: drop `iam.user_password_reset` (no data exists before this plan).
