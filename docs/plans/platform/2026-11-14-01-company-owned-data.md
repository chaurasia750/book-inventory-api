# Plan platform#01: Company-owned data

- **File:** `docs/plans/platform/2026-11-14-01-company-owned-data.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P18-P22; PAC13-PAC17
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** none (`iam#01`, `company#01` and `core#01` are Done)
- **UI plan:** none, it adds no screen or endpoint: it is the mechanism every company-owned table uses
- **Approved by / on:** abhishek / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
After this plan a company-owned entity only has to derive from one base class; the application then stamps its company from the session, hides every other company's rows from every query, and refuses to save a row without a company. Nothing visible changes yet: the first table that uses it is the school master (school#01). You can see it working through the check script, which uses a temporary table.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Core` (the marker `ICompanyOwned` and the base class `CompanyOwnedRoot`, which also gives the audit entry its company, P21), `BookInventory.Infrastructure/Persistence/AppDbContext.cs` (the query filter and the stamping), `CoreMessages` and its catalogue (one message code), `docs/Code_Map.md`. It does not touch `library/BuildingBlocks` and adds no table. Leaves out: every business table (school#01 and later), and any change to the existing platform tables.

## 3. Approach
`CompanyOwnedRoot` extends `AggregateRoot`, holds `CompanyId` with a private setter and one method `AssignCompany(Guid)` that accepts a company once and refuses an empty id or a second company (P20); it implements `IAuditCompanyScope` so the audit trail names the company (P21). `AppDbContext` receives `ICurrentCompany` and, in `ConfigureModel`, adds the named filter `Company` to every entity type that implements `ICompanyOwned` (the existing named filters for soft delete and the hidden account stay); the filter compares with the current company id, and with no company it compares with an empty id, so nothing matches (P19). Before saving, every added `ICompanyOwned` row gets `AssignCompany(current company)`; with no company the save throws `ForbiddenException(core.company_required)` (P20). Per-company uniqueness (P22) is a convention for the entity configurations: a unique index that starts with `company_id`; the check proves it with the temporary table. Schema change: none.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `ICompanyOwned`, `CompanyOwnedRoot` (assign once, audit company) (P18, P20, P21).
- [ ] 2. `AppDbContext`: inject `ICurrentCompany`; add the named query filter `Company` for every company-owned entity (P19).
- [ ] 3. `AppDbContext`: stamp the company of an added row from the session and refuse a save without one (P20).
- [ ] 4. Message code `core.company_required` in `CoreMessages` and its catalogue.
- [ ] 5. Document the convention (a per-company unique index starts with `company_id`, P22) in `docs/Code_Map.md`.
- [ ] 6. Build at 0 warnings, 0 errors; check size limits.
- [ ] 7. Verify every row of section 5 with a temporary company-owned entity, its table created by hand in the check database; the temporary code is never committed.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P18`, `P20`, `PAC13` | 1, 3 | Temporary entity saved in company A with a body carrying company B's id -> `company_id` A (SQL) |
| `P19`, `PAC14` | 2 | Owner of B: `GET` of A's row -> 404 identical to an unknown id; B's list shows none of A's rows; counts agree |
| `P19`, `P20`, `PAC15` | 2, 3, 4 | Super Admin with nothing entered: list empty and `POST` -> 403 `core.company_required`; after entering A: list and `POST` work and the row's company is A |
| `P20`, `PAC16` | 1 | No behaviour of the entity changes `company_id` (a second `AssignCompany` throws; code review of the root) |
| `P21`, `PAC17` | 1 | The audit entry of the temporary row has `company_id` A |
| `P22`, `PAC17` | 5 | The temporary table has a unique index on `(company_id, code)`: the same code in A and B is accepted, twice in A is a unique violation (409) |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The filter is evaluated per context instance from the session, so a background job (no session) sees no company-owned rows; a job that needs them must say which company it works for (a later plan). `IgnoreQueryFilters()` removes all named filters including soft delete; code that needs only to bypass the company filter must say so deliberately and is reviewed. The check uses a hand-made table and a temporary entity class, so no migration is added by this plan.
