# Plan customer#01: Customer master

- **File:** `docs/plans/customer/2026-11-14-01-customer-master.md`
- **Kind:** Backend
- **Spec:** [../../specs/customer.md](../../specs/customer.md) - rules covered: R1-R14; AC1-AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `platform#07` (Done), `school#03` (Approved)
- **UI plan:** customer#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the customer spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner (and Staff, for creating) can keep the shop's credit customers: name, mobile, address, an optional school and class, the opening balance and an optional credit limit, each with a generated code `CUS-0001`; and a bill screen can suggest a customer while the name or mobile is typed. Try it: as an Owner `POST /api/v1/customers` with `{name: "Ramesh Kumar", mobile: "9876500001"}`, then `GET /api/v1/customers/suggest?text=ram`.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Customer` (aggregate, events, message codes), `BookInventory.Application/Customer` (use cases, query service, contracts, catalogue), `BookInventory.Infrastructure/Customer` and `Persistence` (schema `customer`, configuration, migration), `BookInventory.Api/Controllers/Customer`, `CLAUDE.project.md` (the module list gets `customer`, in its own commit), `docs/Code_Map.md`.
Leaves out: the customer ledger, outstanding and opening-balance posting (Ledger module), customer payments (Payment module), the bill and its credit-limit warning (Counter Sale module), Excel import (Q10), any UI (customer#02).
Schema change: yes (new schema `customer`, additive).

## 3. Approach
It follows the vendor master closely: `Customer` derives from `CompanyOwnedRoot`; creation, edit, activate and deactivate are root behaviours that validate R2-R7 (name, mobile normalisation shared with the other masters, money values, the school-class pair); the code comes from `ICompanyCodes.NextAsync("CUS")` (P23). The use case checks what needs another module or another row: the school and class through `ISchoolDirectory` (school R16) and a free mobile (the unique index is the real guard, `409`). The suggestion is a small no-tracking query (R8) capped at 20. Roles follow R9 (Staff may create but not change). Mobile and email rules are reused from where `school` and `vendor` already keep them, not copied.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `Customer` (create, edit, activate, deactivate, values of R2-R7), events, message codes (R2-R7, R14).
- [ ] 2. Persistence: configuration (unique code and mobile per company, indexes), schema `customer`, migration; add `customer` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target `book_inventory` before applying.
- [ ] 3. Use cases: create (code from the series, school and class through the school directory, free mobile), edit, activate, deactivate (R1-R7, R10).
- [ ] 4. Query service: the list (search, filters, sort, paging, the school's name through the directory) and one customer; the suggestion (R8, R11).
- [ ] 5. `CustomersController` with the roles of R9 and the API table.
- [ ] 6. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `R2`, `R5`, `R10`, `AC1` | 1, 3 | Owner of A creates -> 201 `CUS-0001`, ACTIVE, opening 0, no limit, SQL company A; second `CUS-0002`; B's first `CUS-0001`; `code` in the body ignored |
| `R2`, `R3`, `R6`, `R7`, `AC2` | 1, 3 | Name of 1 character, no mobile, 8-digit mobile, opening -1, limit 0 and -5, an amount with three decimals -> 400; mobile `+91 98765 00001` after `9876500001` -> 409 |
| `R4`, `AC3` | 3 | School of A with ` 5TH ` -> stored `5th`; class without school, unknown class, B's school, an inactive school -> 400 |
| `R1`, `AC4` | 4, 5 | Owner of B: list has none of A's, A's id -> 404 identical to an unknown id; `companyId` in query and body ignored |
| `R5`, `R8`, `AC5` | 4 | `suggest?text=ram` -> active Ram... only; `98765` -> by mobile; an inactive one never; at most 20; 1 character -> empty |
| `R9`, `AC6` | 5 | Staff: list, get, suggest 200, create 201; edit, activate, deactivate 403; anonymous 401 |
| `R9`, `AC7` | 5 | Super Admin without a company: empty list, create 403 `core.company_required`; after entering A: list and create |
| `R5`, `AC8` | 3 | Deactivate -> listed under the inactive filter, not suggested; activate restores; twice -> 409 |
| `R11`, `AC9` | 4 | Page size 500 -> 200; search by name, code, mobile; status and school filters; sorts; unknown sort ignored |
| `R12`, `AC10` | 3 | SQL: audit entries `Customer` CREATED and UPDATED with the actor and company A |
| `R13`, `R14` | 1, 2 | DDL shows `numeric(18,2)` for both amounts; no document copies exist yet (nothing to check beyond the column types) |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The migration is additive; rollback: drop schema `customer`. If the owner answers Q3 with "allow the same mobile", the unique index on the mobile becomes a plain index before the plan starts.
