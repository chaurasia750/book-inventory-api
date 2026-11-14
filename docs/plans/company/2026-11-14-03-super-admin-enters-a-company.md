# Plan company#03: The Super Admin enters a company

- **File:** `docs/plans/company/2026-11-14-03-super-admin-enters-a-company.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R13; AC13, AC14, AC15 (and R6, R10 for the entered company)
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `company#01`, `iam#01`
- **UI plan:** company#04
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/company-accounts` by the owner's explicit instruction)

## 1. In one minute
After this plan a Super Admin can "enter" an active company and, from then on until they leave, the server treats their session as working for that company: `GET companies/current` returns it, and every company-owned endpoint of later modules will use it. Try it: as Super Admin `POST /api/v1/companies/{id}/enter`, then `GET /api/v1/companies/current`; `POST /api/v1/companies/leave` clears it.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Company` (enter and leave use cases), `BookInventory.Api/Controllers/Company/CompaniesController.cs` (two endpoints), the existing `GET companies/current`. It uses the `iam` capability that stores `active_company_id` on the session (iam#01); it adds one small table for the record of R12 (see approach). Leaves out: the company picker screen (company#04) and any company-owned business endpoint (later modules).

## 3. Approach
`EnterCompany` loads the company, refuses an inactive one (`409`), calls the `iam` capability to set the session's active company, and records the action through the audit trail with the real actor and the company id (R12, R13). `LeaveCompany` clears it. The new access token claim `cid` (iam R62) is delivered by refreshing the session token in the response, so the UI keeps one token; if the existing refresh path cannot carry the change, this plan stops and asks. Schema change: yes, one insert-only table `company.company_entry` (company, user, action, time): the `core` audit trail does not exist yet, so enter and leave are recorded there (proposed to the owner as decision D10 of the company spec). Only this plan changes the `company` schema right now.

## 4. Tasks (at most 8)
- [x] 1. Use cases `EnterCompany` and `LeaveCompany` (active company only, audit entry with actor and company id) (R13).
- [x] 2. Controller endpoints `POST companies/{id}/enter` and `POST companies/leave` (Super Admin only) returning what the UI needs to continue (the current company and a refreshed access token).
- [x] 3. Make `GET companies/current` answer from `ICurrentCompany` for both company accounts and an entered Super Admin (R6, R10).
- [x] 4. Build at 0 warnings, 0 errors; check size limits and `docs/Code_Map.md`.
- [x] 5. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R13`, `AC13` | 1, 2, 3 | Super Admin: enter A -> `GET companies/current` = A; enter B -> B; leave -> 404; the token claim `cid` follows **Observed (2026-11-14, throw-away DB, 10 migrations applied by the app at start):** nothing entered -> `companies/current` 404; enter A -> 200 with company A, a new token with `cid` = A and a refreshed access cookie; `companies/current` with it -> A (state `Rajasthan`), also with `?companyId=<B>`; the token from before entering -> 404; `active_company_id` = A on the Super Admin session; enter B -> `cid` = B; leave -> 200, company null, no `cid`, `companies/current` -> 404, 0 sessions keep a company; leave again -> 200. |
| `R13`, `AC14` | 2 | Owner and Staff: `POST companies/{id}/enter` -> 403; entering a deactivated company -> 409; unknown id -> 404 **Observed:** Owner and Staff `POST companies/{id}/enter` -> 403, Owner `leave` -> 403; a deactivated company -> 409 `company.not_active`; unknown id -> 404 `company.not_found`; a refused enter wrote no record and left no company on the session. |
| `R12`, `R13`, `AC15` | 1 | After enter and leave, the audit trail shows two entries with the Super Admin's real id and the company id (SQL query) **Observed:** table `company.company_entry` holds 3 rows (ENTERED A, ENTERED B, LEFT B; the second leave added none), each with the Super Admin's id as `user_id` and `created_by`, the LEFT row naming company B. |
| `R6`, `R10` | 3 | Super Admin with nothing entered: `GET companies/current` -> 404; after entering -> the company; a request carrying another `companyId` still answers the entered company **Observed:** see the rows above: no company before entering and after leaving (404), the entered company while entered, and a `companyId` in the query never changes it. |

## 6. Not verified
- "Work with its data" is proven only through `companies/current`: no company-owned business endpoint exists yet.
- The migration `Company_Entries` (one new insert-only table; additive) was applied only to a throw-away database. It was **not** applied to the local development database `book_inventory`; the app applies it at its next start. The SQL was shown to the owner and is waiting for confirmation.
- Whether the `ISessionCompanySelector` token refresh survives a parallel `auth/refresh` of the same session was not exercised.
- An entered company that is deactivated afterwards stays on the Super Admin's session (see risks).

## 7. Risks and notes
A Super Admin session with an entered company that is later deactivated by another Super Admin keeps that company on the session (only the company's own accounts are revoked, R7); business modules must refuse writes to an inactive company, which is for those plans. Rollback of the migration: drop `company.company_entry`.
