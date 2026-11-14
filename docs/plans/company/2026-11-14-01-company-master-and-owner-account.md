# Plan company#01: Company master and its Owner account

- **File:** `docs/plans/company/2026-11-14-01-company-master-and-owner-account.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R1-R12; AC1-AC12
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `iam#01`
- **UI plan:** company#02
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
After this plan a Super Admin can create a company (for example "Children Book Center") together with its Owner login, list and open companies, edit them, and deactivate or reactivate them. The Owner logs in with the generated user id (`childrenbookcenter`) and reads their own company with `GET companies/current`. Try it: `POST /api/v1/companies` as Super Admin, then log in with the returned user id and temporary password.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Company`, `BookInventory.Application/Company`, `BookInventory.Infrastructure/Company` and `Persistence` (schema `company`, migration), `BookInventory.Api/Controllers/Company/CompaniesController.cs`, DI registration. Leaves out: entering a company (company#03), logo, bank account, invoice settings, number series (later features, Q11) and every screen (company#02).

## 3. Approach
`Company` is an aggregate root with a `CompanyStatusHistory` child (insert-only). Business rules (name rules, status transitions, reason length) live in the aggregate. The user id of the Owner is built by a domain rule from the company name (lowercase letters and digits, spaces and symbols removed, Q15); an empty result is rejected. The create use case calls the `iam` Owner capability in the same request transaction, so a failure saves nothing (R4). Deactivation calls the `iam` session-revocation capability from the use case, not from an event handler (handlers run after the save). `company` implements the `iam` port that answers whether a company is active. Reads for the list use `TableNoTracking`, paging and a whitelisted sort. `GET companies/current` reads the company from `ICurrentCompany` only. Schema change: yes (new schema `company`). Only this plan changes the `company` schema right now.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `Company` root (create, edit, deactivate, reactivate, user id rule), `CompanyStatusHistory`, events `CompanyCreated`, `CompanyDeactivated`, `CompanyReactivated`, message codes (R1, R3, R4, R7, R8).
- [ ] 2. Persistence: configurations, schema `company`, unique index on the normalised name, migration; show the SQL and the target database `book_inventory` to the user before applying.
- [ ] 3. Use case `CreateCompany`: validates, creates the company, calls the Owner capability, returns the one-time user id and temporary password (R3, R4).
- [ ] 4. Use cases `EditCompany`, `DeactivateCompany` (revokes sessions through `iam`), `ReactivateCompany` (R2, R7, R8).
- [ ] 5. Query services: paged list with search, status filter and whitelisted sort; detail with status history and the company's accounts read through the `iam` user directory (R9, R11).
- [ ] 6. `CompaniesController` (Super Admin only) and `GET companies/current`; implement the `iam` company-status port (R2, R6, R10).
- [ ] 7. Build at 0 warnings, 0 errors; check size limits and one top-level type per file; update `docs/Code_Map.md` with the new module.
- [ ] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `R5`, `AC4` | 3, 6 | Create A and B with their Owners; Owner of A: `GET companies/current` -> A; token has A's `cid` |
| `R2`, `AC5`, `AC10`, `AC12` | 6 | Owner of A: `GET companies`, `GET/PUT companies/{B}`, `PUT companies/{A}`, `POST companies` -> 403 each; Super Admin with an unknown id -> 404 |
| `R3`, `AC2` | 1, 3 | Create "Children Book Center", then " children book center " -> 409 `company.name_taken`; a name of 1 character -> 400 |
| `R4`, `AC1`, `AC3` | 1, 3 | `POST companies` -> 201 with user id `childrenbookcenter` and a temporary password once; the Owner logs in with them and is forced to change the password; a company whose generated id is already taken -> 409 and `select count(*) from company.company` unchanged |
| `R6`, `AC11` | 6 | Owner of A: `GET companies/current?companyId=<B>` and a `companyId` header -> still A |
| `R7`, `AC6`, `AC7`, `AC8` | 4 | Deactivate A with a reason -> A's Owner token 401 and login refused, B's Owner fine; reactivate -> login works; no reason -> 400; twice -> 409 |
| `R8`, `AC9` | 1, 4 | After create, deactivate, activate: three history rows with from, to, reason, actor, time (SQL query); no update or delete path |
| `R9`, `AC10` | 5 | List with `search`, `status`, `page`, `sort` -> expected rows; `pageSize=500` is capped at 200; detail shows history and the Owner |
| `R10` | 6 | `GET companies/current` returns name, status, state of the caller's company; a Super Admin with nothing entered gets 404 |
| `R11` | 5 | Detail lists the Owner (name, user id, role, status); the Owner's status changes use the existing `iam` endpoints (no new rule) |
| `R12` | 1, 2 | `created_by`, `created_at`, `updated_by`, `updated_at` filled by the framework on every change (SQL query) |

## 6. Not verified
To be filled when the plan is closed (R13 and AC13-AC15 belong to company#03; the staff rules R14 are proven in iam#03).

## 7. Risks and notes
Rollback of the migration: drop schema `company` (no data exists before this plan). The temporary password appears only in the create response and is never logged. The name-uniqueness check races between two parallel creates: the unique index is the real guard and a violation is mapped to `409 company.name_taken`. Two different company names can produce the same user id ("A B" and "AB"): the second is refused with `409` and the Super Admin changes the name (Q15).
