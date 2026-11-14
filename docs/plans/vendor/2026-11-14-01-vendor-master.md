# Plan vendor#01: Vendor master

- **File:** `docs/plans/vendor/2026-11-14-01-vendor-master.md`
- **Kind:** Backend
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R1-R14; AC1-AC12
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `platform#01` (Done), `platform#07` (Draft)
- **UI plan:** vendor#02
- **Approved by / on:** pending (spec R1-R14 are Draft, Q1-Q10 open)
- **Created:** 2026-11-14
- **Base branch:** `docs/school-master` (owner's standing instruction: work on the current branch)

## 1. In one minute
After this plan an Owner can create, list, edit, activate and deactivate the vendors of their company, with bank details and an opening balance, and no company ever sees another's vendors. Try it: as an Owner `POST /api/v1/vendors` with a name and get back `VEN-0001`; `GET /api/v1/vendors` lists it with the account number masked.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Vendor`, `BookInventory.Application/Vendor`, `BookInventory.Infrastructure/Vendor` and `Persistence` (schema `vendor`, migration), `BookInventory.Api/Controllers/Vendor/VendorsController.cs`, `CLAUDE.project.md` (the module list gets `vendor`, in its own commit), `docs/Code_Map.md`, the audit mechanism's secret marker for the account number.
Leaves out: Excel import, the vendor ledger and payments, purchase documents, the Super Admin's screens in `admin`, any UI (vendor#02).

## 3. Approach
`Vendor` derives from `CompanyOwnedRoot`, so its company is stamped and filtered by the platform; the code comes from `ICompanyCodes` (platform#07) with prefix `VEN`. Mobile and email reuse `IdentityNormalizer` like school; GSTIN, IFSC and account-number shapes are value rules inside `VendorRules`, written once. The bank block is a value object of five values that are all present or all absent; the account number carries the audit secret marker and is masked by one function used by the list and the detail. Names are compared in normalised form for R4.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `Vendor` (create, edit, activate, deactivate), `VendorBank` value object, `VendorRules`, the state list (Indian states), events, message codes (R2-R7, R13).
- [ ] 2. Persistence: configuration with the per-company unique indexes, schema `vendor`, migration; add `vendor` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [ ] 3. Use cases `CreateVendor` (code from `ICompanyCodes`), `EditVendor`, `ActivateVendor`, `DeactivateVendor` (R1-R7, R11).
- [ ] 4. Query service: paged list with search, filters and whitelisted sort, masked account number; detail with the full number for the Owner (R5, R10).
- [ ] 5. `VendorsController` with the roles of the API table; Staff refused (R8, R9).
- [ ] 6. Audit: the account number is kept out of the trail's old and new values (R12).
- [ ] 7. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 8. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC3` | 1, 3, 5 | Owner of A creates a vendor -> 201, `company_id` A (SQL); Owner of B: list shows none of A's, `GET` of A's id -> 404; `companyId` in query and body ignored |
| `R11`, `AC2` | 3 | `VEN-0001`, `VEN-0002`; first of B `VEN-0001`; body `code` ignored; deactivate then create -> next number; ten parallel creates -> ten codes |
| `R2`, `R3`, `R4`, `R6`, `AC4`, `AC12` | 1, 3 | Negative balance, 1-character name, bad GSTIN, state outside the list, bad email -> 400; `ABC  publications` after `abc Publications` -> 409; mobiles stored as digits |
| `R5`, `AC5`, `AC6` | 1, 4, 6 | Partial bank block -> 400; account `12345678` -> 400; list and audit trail show `XXXXXX1234`; Owner `GET vendors/{id}` shows it in full |
| `R7`, `AC7` | 3 | Deactivate -> 204, listed under inactive; again -> 409; activate -> 204 |
| `R8`, `R9`, `AC8`, `AC9` | 5 | Staff on every route -> 403; anonymous -> 401; Super Admin with nothing entered -> list empty, `POST` -> 403 `core.company_required`; after entering A it works |
| `R10`, `AC10` | 4 | Search, filters, sorts, `pageSize=500` capped at 200, unknown `orderBy` ignored |
| `R12`, `AC11` | 6 | `GET audit?entityType=Vendor` shows `CREATED` and `UPDATED` with the Owner and company A |
| `R13`, `R14` | 1 | Renaming leaves documents untouched (none exist; the aggregate holds no reference to documents); amounts are `numeric(18,2)` (SQL `\d vendor.vendor`) |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The migration is additive (new schema and table); rollback: drop schema `vendor`. Bank details are the first sensitive field of the system: the check data uses fake numbers only. If the owner answers Q4 with "do not store the account number" the bank block shrinks to holder, bank and IFSC.
