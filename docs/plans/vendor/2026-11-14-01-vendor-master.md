# Plan vendor#01: Vendor master

- **File:** `docs/plans/vendor/2026-11-14-01-vendor-master.md`
- **Kind:** Backend
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R1-R14; AC1-AC12
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `platform#01` (Done), `platform#07` (Approved)
- **UI plan:** vendor#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: "approved hai kro implement")
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `feature/platform-07` (cut from it: vendor#01 depends on platform#07)

## 1. In one minute
After this plan an Owner can create, list, edit, activate and deactivate the vendors of their company, with bank details and an opening balance, and no company ever sees another's vendors. Try it: as an Owner `POST /api/v1/vendors` with a name and get back `VEN-0001`; `GET /api/v1/vendors` lists it with the account number masked.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Vendor`, `BookInventory.Application/Vendor`, `BookInventory.Infrastructure/Vendor` and `Persistence` (schema `vendor`, migration), `BookInventory.Api/Controllers/Vendor/VendorsController.cs`, `CLAUDE.project.md` (the module list gets `vendor`, in its own commit), `docs/Code_Map.md`, the audit mechanism's secret marker for the account number.
Leaves out: Excel import, the vendor ledger and payments, purchase documents, the Super Admin's screens in `admin`, any UI (vendor#02).

## 3. Approach
`Vendor` derives from `CompanyOwnedRoot`, so its company is stamped and filtered by the platform; the code comes from `ICompanyCodes` (platform#07) with prefix `VEN`. Mobile and email reuse `IdentityNormalizer` like school; GSTIN, IFSC and account-number shapes are value rules inside `VendorRules`, written once. The bank block is a value object of five values that are all present or all absent; the account number carries the audit secret marker (never written to the trail) and is masked by one function used by the list and the detail. Names are compared in normalised form for R4.

## 4. Tasks (at most 8)
- [x] 1. Domain: `Vendor` (create, edit, activate, deactivate), `VendorBank` value object, `VendorRules`, the state list (Indian states), events, message codes (R2-R7, R13).
- [x] 2. Persistence: configuration with the per-company unique indexes, schema `vendor`, migration; add `vendor` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [x] 3. Use cases `CreateVendor` (code from `ICompanyCodes`), `EditVendor`, `ActivateVendor`, `DeactivateVendor` (R1-R7, R11).
- [x] 4. Query service: paged list with search, filters and whitelisted sort, masked account number; detail with the full number for the Owner (R5, R10).
- [x] 5. `VendorsController` with the roles of the API table; Staff refused (R8, R9).
- [x] 6. Audit: the account number is kept out of the trail's old and new values (R12).
- [x] 7. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 8. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC3` | 1, 3, 5 | Owner of A creates a vendor -> 201, `company_id` A (SQL); Owner of B: list shows none of A's, `GET` of A's id -> 404; `companyId` in query and body ignored **Observed (2026-11-14, real `book_inventory`, 37 checks, all pass):** `POST vendors` as the ABC Owner -> 201 `VEN-0001` `ACTIVE` (a `code` `HACK-9` in the body ignored); SQL: the row's company is ABC; the Children Owner's list holds none of ABC's vendors; `GET vendors/{ABC id}?companyId=...` as the Children Owner -> 404, same code as an unknown id. |
| `R11`, `AC2` | 3 | `VEN-0001`, `VEN-0002`; first of B `VEN-0001`; body `code` ignored; deactivate then create -> next number; ten parallel creates -> ten codes **Observed:** ABC `VEN-0001`, `VEN-0002`; Children's first `VEN-0001` (even with the same name as ABC's); after deactivating `VEN-0002` the next is `VEN-0003`; ten parallel creates in Children -> ten different codes `VEN-0002` to `VEN-0011`. |
| `R2`, `R3`, `R4`, `R6`, `AC4`, `AC12` | 1, 3 | Negative balance, 1-character name, bad GSTIN, state outside the list, bad email -> 400; `ABC  publications` after `abc Publications` -> 409; mobiles stored as digits **Observed:** balance -5 and 10.125 -> 400 `vendor.invalid_opening_balance`; name `X` -> 400 `vendor.invalid_name`; GSTIN `123` -> 400 `vendor.invalid_gstin`; state `Atlantis` -> 400 `vendor.invalid_state`; email `not-an-email` -> 400 `vendor.invalid_email`; `v01   ALPHA publications` after `V01 Alpha Publications` -> 409 `vendor.name_taken`; GSTIN `10abcde1234f1z5` stored `10ABCDE1234F1Z5`; mobiles `+91 98765 00001`, `0 98765 00001`, `9876500001` all stored `9876500001`. |
| `R5`, `AC5`, `AC6` | 1, 4, 6 | Partial bank block -> 400; account `12345678` -> 400; list shows `XXXXXX1234` and the audit trail holds no account number; Owner `GET vendors/{id}` shows it in full **Observed:** only a holder; account `12345678`; account `12345abcd6789`; IFSC `BAD` -> each 400 `vendor.invalid_bank`; the list shows `XXXXXXXX1234` for account `123456781234` and the whole list response does not contain the number; the Owner's `GET vendors/{id}` shows it in full; the vendor's audit entries exist (`CREATED`) and none holds the number (SQL `like '%123456781234%'` -> 0). |
| `R7`, `AC7` | 3 | Deactivate -> 204, listed under inactive; again -> 409; activate -> 204 **Observed:** deactivate again 409, activate 204, activate twice 409, `VEN-0002` listed under `status=INACTIVE`. |
| `R8`, `R9`, `AC8`, `AC9` | 5 | Staff on every route -> 403; anonymous -> 401; Super Admin with nothing entered -> list empty, `POST` -> 403 `core.company_required`; after entering A it works **Observed:** Staff `GET` and `POST` -> 403, anonymous -> 401; Super Admin with nothing entered: empty list, `POST` -> 403 `core.company_required`; after entering ABC the list holds ABC's vendors and `POST` -> 201. |
| `R10`, `AC10` | 4 | Search, filters, sorts, `pageSize=500` capped at 200, unknown `orderBy` ignored **Observed:** `pageSize=500` -> `pageSize` 200; `orderBy=Code DESC` sorted; `orderBy=BankAccountNumber` -> 200 (ignored); filter `state=Delhi` -> only Delhi; search by GSTIN found exactly one vendor. |
| `R12`, `AC11` | 6 | `GET audit?entityType=Vendor` shows `CREATED` and `UPDATED` with the Owner and company A **Observed (SQL on `core.audit_entry`):** after an edit the vendor has an `UPDATED` entry whose company is ABC; no entry holds the account number. |
| `R13`, `R14` | 1 | Renaming leaves documents untouched (none exist; the aggregate holds no reference to documents); amounts are `numeric(18,2)` **Observed:** `information_schema` shows `opening_balance_amount` as `numeric(18,2)`; the aggregate has no reference to any document. |

## 6. Not verified
- The migration `Vendor_Initial` (new schema `vendor`, table `vendor.vendor`, three indexes, a status check; additive) was applied to the local development database `book_inventory` by the app at its start, on the owner's approval of this plan (its SQL was shown in the session; target `book_inventory` on `localhost:5432`). It is **not** applied to any shared database; the owner applies it there.
- `PUT vendors/{id}` was exercised once (opening balance change, audit `UPDATED`); every field of the edit request was not separately exercised.
- The unique index on `(company_id, name_normalized)` as a database backstop for two parallel creations of the same name was not exercised (the use case's check was).
- The check changed the passwords of the test accounts `abcbookcenter`, `childrenbookcenter` and `childrenbookcenter.one` to `Vendor#Check2026` (they were temporary passwords); the 25 test vendors `V01 ...` stay in the dev database (B27).

## 7. Risks and notes
The migration is additive (new schema and table); rollback: drop schema `vendor`. Bank details are the first sensitive field of the system: the check data uses fake numbers only. If the owner answers Q4 with "do not store the account number" the bank block shrinks to holder, bank and IFSC.
