# Plan platform#01: Company-owned data

- **File:** `docs/plans/platform/2026-11-14-01-company-owned-data.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P18-P22; PAC13-PAC17
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** none (`iam#01`, `company#01` and `core#01` are Done)
- **UI plan:** none, it adds no screen or endpoint: it is the mechanism every company-owned table uses
- **Approved by / on:** abhishek / 2026-11-14
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/school-master` by the owner's explicit instruction)

## 1. In one minute
After this plan a company-owned entity only has to derive from one base class; the application then stamps its company from the session, hides every other company's rows from every query, and refuses to save a row without a company. Nothing visible changes yet: the first table that uses it is the school master (school#01). You can see it working through the check script, which uses a temporary table.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Core` (the marker `ICompanyOwned` and the base class `CompanyOwnedRoot`, which also gives the audit entry its company, P21), `BookInventory.Infrastructure/Persistence/AppDbContext.cs` (the query filter and the stamping), `CoreMessages` and its catalogue (one message code), `docs/Code_Map.md`. It does not touch `library/BuildingBlocks` and adds no table. Leaves out: every business table (school#01 and later), and any change to the existing platform tables.

## 3. Approach
`CompanyOwnedRoot` extends `AggregateRoot`, holds `CompanyId` with a private setter and one method `AssignCompany(Guid)` that accepts a company once and refuses an empty id or a second company (P20); it implements `IAuditCompanyScope` so the audit trail names the company (P21). `AppDbContext` receives `ICurrentCompany` and, in `ConfigureModel`, adds the named filter `Company` to every entity type that implements `ICompanyOwned` (the existing named filters for soft delete and the hidden account stay); the filter compares with the current company id, and with no company it compares with an empty id, so nothing matches (P19). EF caches the model across context instances, so the filter reads `AppDbContext.SessionCompanyId` through the context parameter of the lambda (the documented per-request pattern), never a captured instance; the check proves it by switching companies in one process. Before saving, every added `ICompanyOwned` row gets `AssignCompany(current company)`; with no company the save throws `ForbiddenException(core.company_required)` (P20). Per-company uniqueness (P22) is a convention for the entity configurations: a unique index that starts with `company_id`; the check proves it with the temporary table. Schema change: none.

## 4. Tasks (at most 8)
- [x] 1. Domain: `ICompanyOwned`, `CompanyOwnedRoot` (assign once, audit company) (P18, P20, P21).
- [x] 2. `AppDbContext`: inject `ICurrentCompany`; add the named query filter `Company` for every company-owned entity (P19).
- [x] 3. `AppDbContext`: stamp the company of an added row from the session and refuse a save without one (P20).
- [x] 4. Message code `core.company_required` in `CoreMessages` and its catalogue.
- [x] 5. Document the convention (a per-company unique index starts with `company_id`, P22) in `docs/Code_Map.md`.
- [x] 6. Build at 0 warnings, 0 errors; check size limits.
- [x] 7. Verify every row of section 5 with a temporary company-owned entity, its table created by hand in the check database; the temporary code is never committed.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P18`, `P20`, `PAC13` | 1, 3 | Temporary entity saved in company A with a body carrying company B's id -> `company_id` A (SQL) **Observed (2026-11-14, throw-away DB, temporary entity `ScratchWidget` with a hand-made table `core.scratch_widget`, never committed):** Owner of A `POST` with `companyId` = B in the body -> 200 and `company_id` = A in the table (the body value is ignored by design; the response shows the id before the stamp, so the table is the proof). |
| `P19`, `PAC14` | 2 | Owner of B: `GET` of A's row -> 404 identical to an unknown id; B's list shows none of A's rows; counts agree **Observed:** Owner of B lists only B's `W-1`; `GET` of A's row -> 404 `scratch.not_found`, identical to an unknown id; counts A=2, B=1, table=3. |
| `P19`, `P20`, `PAC15` | 2, 3, 4 | Super Admin with nothing entered: list empty and `POST` -> 403 `core.company_required`; after entering A: list and `POST` work and the row's company is A **Observed:** Super Admin with nothing entered: list `[]` and `POST` -> 403 `core.company_required`, no row written; after entering A: list `W-1, W-2`, create works and the row is A's (table); after switching to B with the same context type: only B's `W-1`, so the filter is read per request, not frozen into EF's cached model. |
| `P20`, `PAC16` | 1 | No behaviour of the entity changes `company_id` (a second `AssignCompany` throws; code review of the root) **Observed:** `AssignCompany(B)` on A's row -> 400 `core.company_immutable`, the row keeps A; the same company again -> 200 (harmless); `CompanyId` has a private setter and only the persistence layer calls `AssignCompany` (code). |
| `P21`, `PAC17` | 1 | The audit entry of the temporary row has `company_id` A **Observed:** `core.audit_entry` for `ScratchWidget` `CREATED`: 3 rows with company A, 1 with company B, 0 without a company. |
| `P22`, `PAC17` | 5 | The temporary table has a unique index on `(company_id, code)`: the same code in A and B is accepted, twice in A is a unique violation (409) **Observed:** unique index on `(company_id, code)`: `W-1` in A and `W-1` in B both 200; `W-1` twice in A -> 409. |

## 6. Not verified
- The 409 of the per-company unique index arrived as the platform's generic conflict (no module code): a real module maps it to its own code (school#01 does for the school code).
- A background job (no HTTP context) reading company-owned rows was not exercised: by design it sees none (see risks).
- `IgnoreQueryFilters([CompanyScope.FilterKey])` (deliberately seeing across companies) has no caller yet and was not exercised.
- The temporary entity and table were deleted; no migration was added by this plan.

## 7. Risks and notes
The filter is evaluated per context instance from the session, so a background job (no session) sees no company-owned rows; a job that needs them must say which company it works for (a later plan). `IgnoreQueryFilters()` removes all named filters including soft delete; code that needs only to bypass the company filter must say so deliberately and is reviewed. The check uses a hand-made table and a temporary entity class, so no migration is added by this plan.
