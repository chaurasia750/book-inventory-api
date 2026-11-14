# Plan school#01: School master

- **File:** `docs/plans/school/2026-11-14-01-school-master.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R1-R15; AC1-AC11
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `platform#01`
- **UI plan:** school#02
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** main (worked on `docs/school-master` by the owner's explicit instruction)

## 1. In one minute
After this plan an Owner can create, list, edit, activate and deactivate the schools of their company and keep each school's class list, Staff can look active schools up, and no company ever sees another's schools. Try it: as an Owner `POST /api/v1/schools {"name":"Delhi Public School","sessionType":"APR_MAR"}` (the answer carries the generated code `SCH-0001`), then `GET /api/v1/schools`.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/School`, `BookInventory.Application/School`, `BookInventory.Infrastructure/School` and `Persistence` (schema `school`, migration), `BookInventory.Api/Controllers/School/SchoolsController.cs`, `CLAUDE.project.md` (the module list, in its own commit). Leaves out: Excel import (Q9), session derivation and cut-offs (Q1), ledger posting of the opening balance, class-wise book lists and every screen (school#02).

## 3. Approach
`School` derives from `CompanyOwnedRoot` (platform#01), so its company is stamped and filtered by the platform; `SchoolClass` is a company-owned child entity (`CompanyOwnedChild`, platform P18: its table carries its own `company_id`, so it is filtered by company and its audit entries name the company): it is added, renamed, reordered and removed only through the root, so the unique and order rules live in one place. A class that keeps its name keeps its row and id (later modules will point at it); a removed class is soft-deleted (P4) and the unique index on `(school_id, name_normalized)` is filtered to live rows, so the same name can be added again. The code comes from `SchoolCodeCounter`, a second company-owned root with one row per company: the creating use case takes a transaction-scoped advisory lock keyed on the company, increases `last_number` and builds `SCH-` plus the number padded to four digits, so parallel creations get different numbers and a number is never reused (R15). Business rules live in the aggregate: name, the state list (Bihar only), mobile and email normalisation, opening balance not negative, class list rules and the status transitions. The use cases only load, call one behaviour and let the request transaction commit. The query service applies the Staff rule (active only) and the whitelisted sort. The module's message catalogue is registered like the others. Money is `decimal` with `numeric(18,2)`; no rounding is involved (an amount is stored as given with two decimals). Schema change: yes (new schema `school`). Only this plan changes the `school` schema right now.

## 4. Tasks (at most 8)
- [x] 1. Domain: `School` (create, edit, classes, activate, deactivate), `SchoolClass`, `SchoolCodeCounter`, enums, events, the state list (Bihar), normalisers and message codes (R2-R8, R13-R15).
- [x] 2. Persistence: configurations with the per-company unique index, schema `school`, migration; add `school` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [x] 3. Use cases `CreateSchool` (with the code counter under its lock), `EditSchool`, `ReplaceClasses`, `ActivateSchool`, `DeactivateSchool` (R1-R8, R15).
- [x] 4. Query service: paged list with search, filters and whitelisted sort, detail, Staff sees active only (R9, R11).
- [x] 5. `SchoolsController` with the roles of the API table (R9, R10).
- [x] 6. Message catalogue and registration.
- [x] 7. Build at 0 warnings, 0 errors; check size limits; update `docs/Code_Map.md`.
- [x] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC3` | 1, 3, 5 | Owner of A creates a school -> 201, `company_id` A (SQL); Owner of B: `GET schools` shows none of A's, `GET schools/{A's id}` -> 404 identical to an unknown id; `companyId` in the query and body ignored **Observed (2026-11-14, on the real local database `book_inventory`, 12 migrations applied by the app at start; owner's instruction):** Owner of A `POST schools` with `code: HACK` and `companyId` = B in the body -> 201, code `SCH-0001`, `company_id` A (SQL); Owner of B lists only `Patna Central`; B `GET` of A's school -> 404 `school.not_found`, identical to an unknown id; B `PUT` on it -> 404; `?companyId=<A>` in the query changes nothing. |
| `R2`, `R15`, `AC2` | 1, 3 | First school of A -> `SCH-0001`, second `SCH-0002`; first of B -> `SCH-0001`; a `code` in the body is ignored; deactivate `SCH-0002` then create -> `SCH-0003`; ten parallel `POST schools` -> ten different codes (SQL `select count(distinct code)`); `PUT` cannot change the code; a 1-character name -> 400; state `Delhi` -> 400, no state -> Bihar **Observed:** A: `SCH-0001`, `SCH-0002`; B: first `SCH-0001`; after deactivating `SCH-0002` the next is `SCH-0003`; `PUT` with `code` in the body leaves `SCH-0001`; a refused creation (1-character name, 400) burns no number (next `SCH-0004`); ten parallel `POST schools` -> ten 201, codes `SCH-0005` to `SCH-0014`, 14 distinct codes in 14 rows; state `Delhi` -> 400 `school.invalid_state`, no state -> Bihar; 1-character name -> 400 `school.invalid_name`. |
| `R3`, `R4`, `R5`, `AC1` | 1 | Default `sale_type` BOTH and `is_gst_default` false; a wrong `sessionType` -> 400 **Observed:** a new school has `saleType` BOTH, `isGstDefault` false, state Bihar; `sessionType` `WRONG` -> 400. |
| `R6`, `AC1`, `AC5` | 1, 3 | New school has the 15 default classes; `PUT classes` with a duplicate -> 400, with three names -> exactly those three in order **Observed:** 15 default classes `Nursery, LKG, UKG, 1st ... 12th` with sort 1-15; `PUT classes` with a duplicate or a case-only duplicate -> 400 `school.duplicate_class`; with `Nursery, 1st, 2nd` -> exactly those three, sort 1-3; table: 3 live rows, the 12 removed ones soft-deleted, the kept names keep their rows; a removed name (`LKG`) can be added again; a re-save with the same names (one with spaces) -> 204. |
| `R7`, `AC4` | 1, 3 | `openingBalanceAmount` -5 -> 400; 1250.5 -> stored 1250.50 **Observed:** `openingBalanceAmount` -5 and 10.125 -> 400 `school.invalid_opening_balance`; 1250.5 -> stored `1250.50` (SQL); bad email -> 400 `school.invalid_email`, bad mobile -> 400 `school.invalid_mobile`; the edit trims the contact person and upper-cases the GSTIN, keeps the code and the class list. |
| `R8`, `R9`, `AC6`, `AC7` | 3, 4, 5 | Deactivate -> 204, hidden from Staff, listed under the inactive filter; again -> 409; activate -> 204; Staff `POST`, `PUT`, `PUT classes`, deactivate, activate -> 403; anonymous -> 401; Staff `GET` of an inactive school -> 404 **Observed:** deactivate -> 204; the inactive filter lists it; Staff list shows active schools only (also with `status=INACTIVE`); Staff `GET` of the inactive school -> 404; deactivate twice -> 409 `school.already_inactive`; activate -> 204; again -> 409 `school.already_active`; Staff `POST`, `PUT`, `PUT classes`, deactivate, activate -> 403 each; anonymous -> 401. |
| `R10`, `AC8` | 5 | Super Admin with nothing entered: list empty, `POST` -> 403 `core.company_required`; after `companies/{A}/enter`: list shows A's, create works **Observed:** Super Admin with nothing entered: list empty and `POST` -> 403 `core.company_required`; after `companies/{A}/enter`: the list shows A's 14 schools and `POST` creates `SCH-0015` in A (SQL). |
| `R11`, `AC9` | 4 | Search by name, code, contact person, mobile; filters; sorts; `pageSize=500` capped at 200; an unknown `orderBy` ignored **Observed:** search by name, by code in any case, by contact person and by mobile; filters by session type (2), sale type and state; sort by code desc (`SCH-0015, 0014, 0013`) and created asc; `pageSize=500` -> 200; `orderBy=PasswordHash` -> 200 (ignored). |
| `R12`, `AC10` | 1 | `GET audit?entityType=School` as Owner shows `CREATED` and `UPDATED` entries with the Owner as actor and company A **Observed:** `GET audit?entityType=School&entityId=<id>` as the Owner shows `CREATED` and `UPDATED` entries, all with company A, the Owner as actor on the updates; `SchoolClass` entries (`CREATED`, `UPDATED`, `DELETED`) are visible to the Owner too, because the class rows carry the company (see section 7). |
| `R13`, `AC11` | 1 | Mobiles `+91 98765 00001`, `0 98765 00001`, `9876500001` -> stored `9876500001`; an invalid email -> 400 **Observed:** mobiles `+91 98765 00001`, `0 98765 00001`, `9876500001`, `91-98765-00001` all stored `9876500001`; the email ` Principal@DPS.Edu ` is stored `principal@dps.edu`. |
| `R14` | 1 | Editing the name leaves existing documents untouched (no documents exist yet: proven by design, the aggregate has no reference to documents) **Observed:** no documents exist yet; the aggregate holds no reference to any document, so renaming cannot touch one (by design, nothing to run). |

## 6. Not verified
- The migration `School_Initial` (new schema `school`, three tables, indexes; additive) was applied to the local development database `book_inventory` by the app at its start, at the owner's explicit instruction to test on that database. The SQL was shown to the owner. It has **not** been applied to any shared, staging or production database.
- Two real concurrent creations from two different company Owners (different advisory locks) were not exercised; ten parallel creations in one company were.
- The `company#01`, `iam#03`, `company#03` and `core#01` checks were not repeated after the company-owned change to `AppDbContext` (platform#01 and this plan's 65 checks ran on top of it).
- Staff cannot create schools by design (Q8); BRD section 3 says Staff may, which the owner decided against (D5): the BRD is still to be updated by the owner.

## 7. Risks and notes
Rollback of the migration: drop schema `school` (no data exists before this plan). The default class list is a domain constant (Q2); changing it later does not change existing schools. The first run of the checks found that the audit entries of a class carried no company, so an Owner could not see them; the fix was to make `SchoolClass` company-owned (a `CompanyOwnedChild` base added to platform, `company_id` on the child table), which the migration of this plan includes. Parallel creations are serialised by the advisory lock on the counter and the unique index on `(company_id, code)` is the second guard. Rollback of a failed creation also rolls back the counter, so a number is not burnt by a refused request.
