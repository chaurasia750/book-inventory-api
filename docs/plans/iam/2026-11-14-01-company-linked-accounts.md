# Plan iam#01: Accounts that belong to a company

- **File:** `docs/plans/iam/2026-11-14-01-company-linked-accounts.md`
- **Kind:** Backend
- **Spec:** [../../specs/iam.md](../../specs/iam.md) - rules covered: R59-R64, R68, R69 (storage and capability); AC30-AC35, AC39, AC40
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** none
- **UI plan:** iam#02
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/company-accounts` by the owner's explicit instruction; that branch sits on `chore/add-company-module`)

## 1. In one minute
After this plan an `iam` account can belong to a company and have the role `OWNER` or `STAFF`; company accounts log in with a user id only; the access token carries the company id; and an account whose company is inactive cannot log in. `iam` offers the `company` module three capabilities: create the Owner, revoke every session of a company's accounts, and remember an entered company on a Super Admin session. You can see it with a check script: create an Owner through the capability, log in with the user id, decode the token (`cid` is there).

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Iam` (roles, account, factory), `BookInventory.Application/Iam` (capabilities, ports, login rule, messages), `BookInventory.Infrastructure/Iam` and `Persistence` (two columns, role check, migration), `BookInventory.Api/Auth` (token claim). It does not touch `library/BuildingBlocks`: the company id is read through an app-level `ICurrentCompany` that wraps the claim. Leaves out: Staff creation and password resets (iam#03), the company master and its API (company#01, company#03) and every screen.

## 3. Approach
`iam` stays independent of `company`: it stores plain ids and asks a port, `ICompanyStatusReader`, whether a company is active; `company` implements it in company#01 (until then a fake that answers "active" lives only in the check script, never committed). The Owner is created by a capability that follows `IMemberUserRegistration` (same transaction, business exceptions roll back); the user id is supplied by the caller. Login for a company account matches the user id only (R64). Session revocation reuses `UserSessionRevoker`. Staff is denied by default simply because no existing endpoint names the role (R68); this plan proves it. Schema change: yes (`app_user.company_id`, `user_session.active_company_id`, `OWNER` and `STAFF` in the role check). Only this plan changes the `iam` schema right now (iam#03 adds none).

## 4. Tasks (at most 8)
- [x] 1. Domain: roles `Owner` and `Staff`; `AppUser.CompanyId` (private set, set only by a new `CreateOwner` factory); guards so neither role is created by staff creation or reached by a role change (R59, R60).
- [x] 2. Persistence: map `company_id` and `user_session.active_company_id` (nullable, indexed), extend the role check constraint, generate the migration, show the SQL and the target database `book_inventory` to the user before applying.
- [x] 3. Application: capability `IOwnerAccountCreation` (user id given, email and mobile optional, temporary password once, uniqueness per R1) reusing the temporary-password generator and identity checks (R61).
- [x] 4. Application: login by user id only for company accounts (R64); port `ICompanyStatusReader`; login refuses an account of an inactive company (generic error before the correct password, `iam.auth.company_inactive` after it; fail closed when no module provides the status), and the existing sessions end through the revoker, so the next request answers 401 without a per-request lookup; capability `ICompanySessionRevoker` (R63).
- [x] 5. Token and session: `cid` claim from the account's company or, for a Super Admin session, from `active_company_id`; `ICurrentCompany`; capability to set and clear the active company of a `SUPER_ADMIN` session only (R62, R69).
- [x] 6. Messages: `iam.auth.company_inactive` and the new validation codes in `IamMessages` and the catalogue.
- [x] 7. Build at 0 warnings, 0 errors; check the new files against the size limits and `docs/Code_Map.md` (one top-level type per file).
- [x] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R59` | 1, 2 | Create an Owner -> `app_user.company_id` set, email and mobile null; the Super Admin row has `company_id` null; no update path changes it (SQL query) **Observed (2026-11-14, throw-away DB, 7 migrations applied by the app at start):** Owner row `OWNER|ACTIVE|must_change=t|company set|email null|mobile null`; Super Admin rows with a company: 0; `CompanyId` has a private setter and is set only by the factory (code). |
| `R60`, `AC32` | 1 | Staff creation (R2) with role `OWNER` or `STAFF` -> 400; role change of an Owner or to `OWNER`/`STAFF` -> 409 (script) **Observed:** `POST users` role OWNER and STAFF -> 400 `iam.user.role_not_staff`; `change-role` to OWNER and of an Owner -> 400 `iam.user.company_role_immutable` (the plan said 409; 400 matches how `MEMBER` is already handled). |
| `R61`, `AC30` | 3 | Call the capability with a new user id -> `ACTIVE`, `must_change_password = true`, password returned once; a taken user id -> 409 and no new row (SQL count before and after); first login allows only change-password **Observed:** capability -> 200, password returned once; taken user id -> 409 `iam.user.duplicate_username`, rows 1 -> 2 (only the first Owner added); first login -> `mustChangePassword=true`; `PUT auth/profile` -> 403 `iam.auth.password_change_required`; change-password -> 200. |
| `R62`, `AC31` | 5 | Log in as Owner -> decoded token has `cid`; the Super Admin token has none **Observed:** Owner and Staff tokens carry `cid` = the company id; the Super Admin token has claims `amr,aud,exp,iss,name,nbf,pwd_change,role,sid,sub` and no `cid`. |
| `R63`, `AC33`, `AC34` | 4, 6 | Fake status reader answers inactive -> correct password: `iam.auth.company_inactive`; wrong password: generic error; old token: 401; reader active again -> login works **Observed:** reader inactive: correct password -> 401 `iam.auth.company_inactive`; wrong password -> 401 `iam.auth.invalid_credentials`, identical to an unknown user; log result `FAILED_COMPANY_INACTIVE` (1 row); reader active -> login 200; after `ICompanySessionRevoker` the old token -> 401 and the login-log rows closed with `COMPANY_DEACTIVATED` (2 rows). |
| `R64`, `AC35` | 4 | Owner with a stored email logs in with the user id -> success; with the email as identifier -> the generic error, byte-identical to an unknown id **Observed:** Owner with email `owner1@example.test`: that email as identifier -> 401 `iam.auth.invalid_credentials`; the user id -> 200. |
| `R68`, `AC39` | 8 | A Staff-role token (account created by script) calling `users`, `POST users`, `login-logs` -> 403; `PUT auth/profile` and `auth/change-password` -> success **Observed:** Staff token: `GET users`, `POST users`, `GET login-logs` -> 403 each; `GET auth/me` -> 200; `PUT auth/profile` -> 204 (success). |
| `R69`, `AC40` | 5 | Call the capability for a Super Admin session -> next token has `cid`; clear -> none; for an Owner session -> rejected **Observed:** Super Admin: capability then a new token -> `cid` = company, `ICurrentCompany` = company, `active_company_id` stored on exactly 1 session; leave -> no `cid`; the same call from an Owner session -> 403 and 0 rows with a company on it. |

## 6. Not verified
- The HTTP endpoints that call the capabilities (enter, leave, create company) are company#01 and company#03; here a temporary probe controller (never committed, deleted) called them.
- `ICompanyStatusReader` was a fake answering from a flag; the real implementation arrives with company#01.
- The migration was applied only to a throw-away database (created and dropped by the check). It was **not** applied to the local development database `book_inventory`; the app applies it at its next start (`Database:MigrateOnStartup`). The SQL was shown to the owner and is waiting for confirmation.
- Concurrent creation of the same user id (two parallel requests): the unique index is the guard and was not exercised.
- UI behaviour is iam#02.

## 7. Risks and notes
Rollback of the migration: drop the two columns and the extra check values. A request check of the company status must stay cheap (one indexed lookup, no extra transaction). The Staff endpoints themselves are iam#03; here Staff exists only as a role value and a check-script account.
