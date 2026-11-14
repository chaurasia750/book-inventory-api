# Plan 0006: iam - account management (Super Admin)

- **Number:** 0006
- **Module:** `iam` (primary) ; also touches: none
- **Spec:** [docs/specs/iam.md](../specs/iam.md) - R2-R6, R28-R41, R57, R58 ; AC9, AC11-AC16, AC20, AC24, AC26, AC27 ; depends on Plans 0003-0005
- **Status:** Done
- **Created:** 2026-09-26   **Completed:** 2026-09-27

## 1. Goal

A Super Admin can create staff, find and inspect any (visible) user, and manage their lifecycle: edit, activate, deactivate, suspend (with auto-lift), approve/reject agents, change staff roles, unlock, force logout, force password change, send a reset link, release a rejected identity, and see status/role history and sessions. All of it through API and the admin UI.

## 2. Out of scope

Login-log search and self-service profile (0007), agent registration itself (`mlm`), business effects of suspension on commissions/withdrawals (`mlm`/`finance`), system account specifics (0009).

## 3. Approach

**Api:**
- Application use cases (thin, calling the `AppUser` behaviours from 0003): `CreateStaff` (generates the temp password, returns it once, R57), `ListUsers` (search name/username/email/mobile, filters role/status/locked, sort, paged; through `IUserDirectory`, so the hidden account never appears), `GetUser` (profile, status+reason+until, lock state; 404 for unknown or hidden id), `UpdateUser` (name/email/mobile, R35), `Activate/Deactivate/Suspend/Approve/Reject` with reasons (R28-R31), `ChangeRole` (staff only, sessions revoked, history, R6), `Unlock`, `ForceLogout`, `ForcePasswordChange`, `SendResetLink`, `ReleaseIdentity` (R58), `GetStatusHistory`, `ListUserSessions`/`RevokeUserSession`.
- Guards implemented in the domain and asserted by the use cases: nobody changes their own status/role (R29), last visible active Super Admin protected (R5), invalid transitions -> 409.
- Every status change writes `UserStatusHistory`, closes the user's sessions and login-log rows with the matching reason (R31, R24), raises the events (R40).
- Hosted service lifting due suspensions to `ACTIVE` with actor `SYSTEM` (R32); login already lifts lazily (Plan 0004).
- `UsersController` with `[Authorize(Roles = "SUPER_ADMIN")]` on all actions; routes per iam §5.

**Ui (`admin` app, Super Admin only):**
- User management page (existing shell filled): table with search box, role/status/locked filters, sort, pagination (`ui` table/data-grid), status badges, "created/last login" columns.
- Create-staff modal (name, username, email, mobile, role) showing the temporary password once in a copy dialog.
- User detail page with tabs: profile (edit), status (current state + history timeline), sessions (revoke), and a link to the login log (0007). Action buttons per status with confirmation/reason dialogs: suspend (reason + optional until date), deactivate, activate, approve/reject, change role, unlock, force logout, force password change, send reset link, release identity.
- Sidebar entries by role; `roleGuard(['SUPER_ADMIN'])` on the routes; all texts from the catalogue.

## 4. Tasks

- [x] 1. Use cases and DTOs for create/list/get/update with the temp password and search/filter/paging
- [x] 2. Status use cases (activate/deactivate/suspend/approve/reject), history, session closing, events, lifting hosted service
- [x] 3. Role change, unlock, force logout, force password change, reset link, release identity, admin session endpoints
- [x] 4. `UsersController`, authorization, error mapping, message codes
- [x] 5. Ui: list page, create modal, detail page, action dialogs, guards/menu, catalogue
- [x] 6. Verify by hand (section 5); update spec/plan status and links

## 5. Verification

Run on 2026-09-27 against throw-away databases (created, migrated and dropped by the scripts), the API started from a separate build folder with the logging sender. The backend was written by another agent and reviewed here; the review's findings were fixed and re-run (below). Build: `dotnet build src/BookInventory.Api` 0 warnings, 0 errors (the solution build was blocked by a dev API holding the DLLs); `eslint` on the changed UI files clean; the public-site and admin apps built in the staging configuration. Scratch scripts `api-check7.sh`, `ui-real3.sh`, `run6.mjs` are not committed.

| Rule / AC | Input | Observed |
| --- | --- | --- |
| R12 (first non-auth check) | `GET users` with the seeded account's must-change token; after the change | `403 iam.auth.password_change_required`; then `200` |
| R27, R44 | a SALES_CRM staff user calls `GET users` | `403` |
| R2, R57, AC26 | create a SALES_CRM staff account | `201` with the temporary password once (14 characters, meets the policy); `GET users/{id}` never contains it; the first login has `mustChangePassword: true`; the response carries `Cache-Control: no-store` |
| R1, AC15 | duplicate e-mail (other casing), user name, mobile; role `MEMBER`; reserved name `admin` | `409 duplicate_email`, `duplicate_username`, `duplicate_mobile`; `400 role_not_staff`; `400 reserved_username` |
| R42-R44, AC20 | a hidden system account row present | list total excludes it; `GET users/{id}` `404` (same as an unknown id); deactivate `403 iam.system_account.immutable`; force-logout `404`; searching `system` finds nothing |
| R28-R31, AC11, AC13 | self-suspend; suspend without a reason, with a 3-character reason, with a past end date; suspend; suspend a suspended user; deactivate; suspend an inactive user; reject a non-pending user | `409 self_status_change` (was `400`, fixed to match AC13); `400 reason_required` x2; `400 suspension_end_in_past`; `204`; `409 invalid_transition` x3 |
| AC11 | the suspended user's token; login with the right password | `401`; `401 iam.auth.account_suspended` (no reason in the body) |
| R33 | status timeline | `GET users/{id}/status-history` lists the changes newest first with the actor's name; `user_status_history` rows `ACTIVE->SUSPENDED`, `SUSPENDED->ACTIVE`, `ACTIVE->INACTIVE` |
| R32, AC12 | `suspended_until` set in the past, then login | `200`, status `ACTIVE`, history row `SUSPENDED->ACTIVE` with a null actor (SYSTEM) |
| R6, AC24 | own role; no reason; to `MEMBER`; `SALES_CRM->ACCOUNTS` | `400 self_role_change`, `role_change_reason_required`, `member_role_immutable`; `204`, the user's old token `401`, `user_role_history` row |
| R34 | 5 wrong passwords, then unlock | `423`; `204`; login works |
| R36 | force logout; force password change; reset link; reset link for a suspended user | `204`, the token `401`, session reason `FORCED_LOGOUT`; `must_change_password = true`; a `PASSWORD_RESET` notification for the address on file; `409 reset_link_not_allowed` |
| R58, AC27 | a rejected agent holds an e-mail; create staff with it; release; again; release a non-rejected user | `409 duplicate_email`; `204`; `201`; `409 invalid_transition` |
| R35, R41 | edit another user; edit own record; edit to a taken e-mail | `204`; `400 self_action_forbidden`; `409 duplicate_email` |
| R38, AC16 | role filter with a real value and with `BOGUS`; `approve`/`activate` without a body | the filter answers (see the UI run); `400` for `BOGUS` (was `500`, fixed); `204` without a body |
| UI (Edge, same origin, real API) | Super Admin signs in; opens Users; creates a staff user; searches; filters; a duplicate e-mail; opens the detail; suspends without and with a reason; activates; the new staff user signs in; a non-Super-Admin opens the page | the sidebar shows Users; the list shows real rows (the sample rows are gone); the temporary password dialog shows a 14-character password once and closing it removes it from the page; search narrows the table to 1 row; role and status filters answer through the API (SALES_CRM 1, ACCOUNTS 0, REJECTED 0, all 2); the duplicate shows the catalogue text; suspending without a reason shows the `reason_required` text, with a reason the status becomes Suspended with the reason and a history row, and only Activate is offered; Activate returns to Active with the live-account actions; the staff user is forced to change the temporary password; `/admin/users` for a non-Super-Admin goes to `unauthorized` |

**Defects found in the review and fixed:** the enum query binder threw on single-word members (`Accounts`, `Member`), so any `?role=` or `?status=` answered `500` (now a deduplicated map; an invalid value is `400`); the lazy suspension lift at login left no timeline row (another agent's `LoginRefusalService` now records it with SYSTEM); `approve`/`activate` without a body were `400` (the body is optional); identity uniqueness lookups skipped the hidden account (a staff e-mail equal to its placeholder would have hit the unique index as a `500`); `FindForManagementAsync` and `FindUserByIdAsync` were the same query (now `FindByIdIncludingHiddenAsync`); the suspension-lift and expired-session sweepers repeated the same timer/scope/error skeleton (now `PeriodicSweeper`); `POST users` had no `Cache-Control: no-store`; self status change was `400` instead of `409`.

**NOT verified:** the 5-minute timer of `SuspensionLiftSweeper` (its logic is the code the lazy-lift check exercises); the remaining action dialogs in the browser (reject, deactivate, change role, unlock, force logout, force password change, reset link, release identity, revoke session) - each was verified through the API and they share one dialog; the last-Super-Admin refusal (R5) through the API (unreachable with an active Super Admin as the actor, verified in the domain in Plan 0003; two admins deactivating each other at the same moment can still both pass - a known limit); `NotificationWorker` was not moved onto `PeriodicSweeper` (different loop shape).

## 6. Risks and notes

- The reset-link action queues a `PASSWORD_RESET` notification through the `core` outbox (Plans 0011 and 0005); the logging sender is used until a provider is configured.
- Approve/reject are callable here by a Super Admin; the agent-onboarding workflow that calls them programmatically lives in `mlm`.
