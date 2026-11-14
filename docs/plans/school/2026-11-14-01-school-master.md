# Plan school#01: School master

- **File:** `docs/plans/school/2026-11-14-01-school-master.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R1-R15; AC1-AC11
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `platform#01`
- **UI plan:** school#02
- **Approved by / on:** abhishek (owner) / 2026-11-14
- **Created:** 2026-11-14   **Completed:** -

## 1. In one minute
After this plan an Owner can create, list, edit, activate and deactivate the schools of their company and keep each school's class list, Staff can look active schools up, and no company ever sees another's schools. Try it: as an Owner `POST /api/v1/schools {"name":"Delhi Public School","sessionType":"APR_MAR"}` (the answer carries the generated code `SCH-0001`), then `GET /api/v1/schools`.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/School`, `BookInventory.Application/School`, `BookInventory.Infrastructure/School` and `Persistence` (schema `school`, migration), `BookInventory.Api/Controllers/School/SchoolsController.cs`, `CLAUDE.project.md` (the module list, in its own commit). Leaves out: Excel import (Q9), session derivation and cut-offs (Q1), ledger posting of the opening balance, class-wise book lists and every screen (school#02).

## 3. Approach
`School` derives from `CompanyOwnedRoot` (platform#01), so its company is stamped and filtered by the platform; `SchoolClass` is a child entity (insert, rename, reorder and remove through the root, so the unique and order rules live in one place). The code comes from `SchoolCodeCounter`, a second company-owned root with one row per company: the creating use case takes a transaction-scoped advisory lock keyed on the company, increases `last_number` and builds `SCH-` plus the number padded to four digits, so parallel creations get different numbers and a number is never reused (R15). Business rules live in the aggregate: name, the state list (Bihar only), mobile and email normalisation, opening balance not negative, class list rules and the status transitions. The use cases only load, call one behaviour and let the request transaction commit. The query service applies the Staff rule (active only) and the whitelisted sort. The module's message catalogue is registered like the others. Money is `decimal` with `numeric(18,2)`; no rounding is involved (an amount is stored as given with two decimals). Schema change: yes (new schema `school`). Only this plan changes the `school` schema right now.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `School` (create, edit, classes, activate, deactivate), `SchoolClass`, `SchoolCodeCounter`, enums, events, the state list (Bihar), normalisers and message codes (R2-R8, R13-R15).
- [ ] 2. Persistence: configurations with the per-company unique index, schema `school`, migration; add `school` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [ ] 3. Use cases `CreateSchool` (with the code counter under its lock), `EditSchool`, `ReplaceClasses`, `ActivateSchool`, `DeactivateSchool` (R1-R8, R15).
- [ ] 4. Query service: paged list with search, filters and whitelisted sort, detail, Staff sees active only (R9, R11).
- [ ] 5. `SchoolsController` with the roles of the API table (R9, R10).
- [ ] 6. Message catalogue and registration.
- [ ] 7. Build at 0 warnings, 0 errors; check size limits; update `docs/Code_Map.md`.
- [ ] 8. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC3` | 1, 3, 5 | Owner of A creates a school -> 201, `company_id` A (SQL); Owner of B: `GET schools` shows none of A's, `GET schools/{A's id}` -> 404 identical to an unknown id; `companyId` in the query and body ignored |
| `R2`, `R15`, `AC2` | 1, 3 | First school of A -> `SCH-0001`, second `SCH-0002`; first of B -> `SCH-0001`; a `code` in the body is ignored; deactivate `SCH-0002` then create -> `SCH-0003`; ten parallel `POST schools` -> ten different codes (SQL `select count(distinct code)`); `PUT` cannot change the code; a 1-character name -> 400; state `Delhi` -> 400, no state -> Bihar |
| `R3`, `R4`, `R5`, `AC1` | 1 | Default `sale_type` BOTH and `is_gst_default` false; a wrong `sessionType` -> 400 |
| `R6`, `AC1`, `AC5` | 1, 3 | New school has the 15 default classes; `PUT classes` with a duplicate -> 400, with three names -> exactly those three in order |
| `R7`, `AC4` | 1, 3 | `openingBalanceAmount` -5 -> 400; 1250.5 -> stored 1250.50 |
| `R8`, `R9`, `AC6`, `AC7` | 3, 4, 5 | Deactivate -> 204, hidden from Staff, listed under the inactive filter; again -> 409; activate -> 204; Staff `POST`, `PUT`, `PUT classes`, deactivate, activate -> 403; anonymous -> 401; Staff `GET` of an inactive school -> 404 |
| `R10`, `AC8` | 5 | Super Admin with nothing entered: list empty, `POST` -> 403 `core.company_required`; after `companies/{A}/enter`: list shows A's, create works |
| `R11`, `AC9` | 4 | Search by name, code, contact person, mobile; filters; sorts; `pageSize=500` capped at 200; an unknown `orderBy` ignored |
| `R12`, `AC10` | 1 | `GET audit?entityType=School` as Owner shows `CREATED` and `UPDATED` entries with the Owner as actor and company A |
| `R13`, `AC11` | 1 | Mobiles `+91 98765 00001`, `0 98765 00001`, `9876500001` -> stored `9876500001`; an invalid email -> 400 |
| `R14` | 1 | Editing the name leaves existing documents untouched (no documents exist yet: proven by design, the aggregate has no reference to documents) |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Rollback of the migration: drop schema `school` (no data exists before this plan). The default class list is a domain constant (Q2); changing it later does not change existing schools. Parallel creations are serialised by the advisory lock on the counter and the unique index on `(company_id, code)` is the second guard. Rollback of a failed creation also rolls back the counter, so a number is not burnt by a refused request.
