# Plan iam#03: The Owner creates and manages Staff accounts

- **File:** `docs/plans/iam/2026-11-14-03-owner-manages-staff-accounts.md`
- **Kind:** Backend
- **Spec:** [../../specs/iam.md](../../specs/iam.md) - rules covered: R65, R66, R67; AC36, AC37, AC38
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `iam#01`
- **UI plan:** iam#04
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
After this plan an Owner can create logins for their company's people (`childrenbookcenter.ramesh` style), see and manage only their own company's Staff, and reset a Staff password to a new temporary password shown once. The Super Admin can reset any Owner or Staff password the same way. Try it: log in as an Owner, `POST /api/v1/staff {fullName, shortName}`, then log in as the new Staff with the returned password.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Iam` (`CreateStaff` factory, temporary-password reset behaviour, reset record), `BookInventory.Application/Iam` (Staff use cases, query service scoped by `ICurrentCompany`), `BookInventory.Api/Controllers/Iam/StaffController.cs`, the `users` controller for the Super Admin reset. Also touches `BookInventory.Infrastructure/Persistence` (one new table and its migration). Leaves out: what Staff may do in business modules (Q14, decided when those modules are built) and Staff screens (iam#04).

## 3. Approach
Every Staff query and command is scoped by the Owner's company from `ICurrentCompany`, never from input (R66); a record outside that scope answers `404`. The short name is validated (letters and digits, 2-20 characters) and joined to the Owner's user id; uniqueness is the existing R1 check. A reset sets a new generated password, `must_change_password`, revokes sessions through the existing revoker and writes an actor/time record. Schema change: yes, one new insert-only table `iam.user_password_reset` (`user_id`, `reset_by`, `reset_at`), written when a reset happens (R67). Because only one schema-changing plan per module may be open, this plan starts after iam#01 is `Done` (the `Depends on` line).

## 4. Tasks (at most 8)
- [ ] 1. Domain: `AppUser.CreateStaff` (role `Staff`, company of the Owner, `must_change_password`), and a reset-to-temporary-password behaviour on the aggregate (R65, R67).
- [ ] 2. Persistence: `UserPasswordReset` child entity, configuration and migration; show the SQL and the target database `book_inventory` to the user before applying (R67).
- [ ] 3. Use case `CreateStaff` for an Owner: validates the short name, builds the user id, returns the one-time password (R65).
- [ ] 4. Query service and use cases for the Owner: list, detail, sessions, deactivate, activate, unlock, force logout, reset password, all scoped to the Owner's company and to `Staff` (R66).
- [ ] 5. `StaffController` (`OWNER` only) and `POST users/{id}/temporary-password` for the Super Admin (R66, R67).
- [ ] 6. Messages for the new error codes in `IamMessages` and the catalogue.
- [ ] 7. Build at 0 warnings, 0 errors; check size limits and `docs/Code_Map.md`.
- [ ] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R65`, `AC36` | 1, 3, 5 | Owner of A: `POST staff {fullName: "Ramesh", shortName: "ramesh"}` -> 201, user id `<ownerid>.ramesh`, password once, `company_id` = A (SQL), first login forces a change; same short name again -> 409; `role: OWNER` in the body is ignored or rejected |
| `R66`, `AC37` | 4, 5 | Owner of A lists only A's Staff; `GET staff/{id}` of B's Staff, of A's Owner, of a Super Admin -> 404 identical to an unknown id; deactivate without a reason -> 400, with a reason -> 200 and the Staff's old token -> 401; unlock and force logout work |
| `R67`, `AC38` | 1, 2, 5 | Owner of A: `POST staff/{id}/temporary-password` -> new password once, old password and old sessions rejected, `must_change_password = true`, a record with actor and time; Owner of B -> 404; Super Admin: `POST users/{ownerId}/temporary-password` -> works |
| company `R14`, iam `R68`, `AC39` | 5, 8 | A Staff created through `POST staff` logs in; calling `companies`, `users`, `staff` -> 403; own profile and change-password -> success |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The temporary password appears only in the response and is never logged. An Owner resetting their own password is not allowed through this route (the Owner uses change-password). Rollback of the migration: drop `iam.user_password_reset` (no data exists before this plan).
