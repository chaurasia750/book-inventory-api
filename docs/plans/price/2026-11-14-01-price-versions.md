# Plan price#01: Price versions

- **File:** `docs/plans/price/2026-11-14-01-price-versions.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R1-R9, R15-R17; AC1-AC8, AC12
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `platform#01` (Done), `item#03` (Approved)
- **UI plan:** price#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan an Owner can add and edit price versions of an item (MRP, sale price, purchase price, effective date), see which version is the default, copy a whole version to a new one for the new year, and see the price list; Staff can read prices without the purchase price; no company sees another's prices. Try it: as an Owner `POST /api/v1/items/{itemId}/prices` with `{versionName: "2026-27", effectiveFrom: "2026-04-01", mrpAmount: 125, saleAmount: 112.5, purchaseAmount: 100}`, then `GET /api/v1/items/{itemId}/prices` shows it as the default.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Price`, `BookInventory.Application/Price`, `BookInventory.Infrastructure/Price` and `Persistence` (schema `price`, migration), `BookInventory.Api/Controllers/Price/PricesController.cs`, `CLAUDE.project.md` (the module list gets `price`, in its own commit), `docs/Code_Map.md`.
Leaves out: bulk price change (price#03), any UI (price#02), Excel upload, the purchase warning, picking a version on a document.
Schema change: yes (new schema `price`, additive).

## 3. Approach
`PriceVersion` derives from `CompanyOwnedRoot`; its item is a plain id, checked through the item directory (item#03). `PriceRules` (name, amounts with at most two decimals, not negative) and the default-version rule (latest effective date not after the business date, ties by creation) are written once, the default as a small domain function over a list of versions so the list query and the single-item read use the same rule. "Today" comes from `TimeProvider` converted to `Asia/Kolkata`. Staff reads are projected without the purchase price in the query service (the Owner's projection is a separate record, so the amount cannot leak by accident). The price list asks the item directory for a page of item ids (search, filters, sort) and then loads those items' versions in one query to pick each default. Copy is one request transaction over many versions (decision D4). History is not built here: it is the audit trail of `core` (R15).

## 4. Tasks (at most 8)
- [ ] 1. Domain: `PriceVersion` (add, edit), `PriceRules`, the default-version rule, events, message codes (R2, R3, R4, R17).
- [ ] 2. Persistence: configuration with the per-company unique indexes, schema `price`, migration; add `price` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [ ] 3. Use cases `AddVersion`, `EditVersion`, `CopyVersion` (R1, R2, R3, R8).
- [ ] 4. Query service: an item's versions with the default marked, Staff without purchase price, the price list with search, filters and sort through the item directory (R4, R5, R9).
- [ ] 5. `PricesController` with the roles of the API table (R5, R6).
- [ ] 6. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC4` | 3, 5 | Owner of A adds `2026-27` to A's item -> 201, `company_id` A (SQL); same name in another case -> 409; negative value, three decimals, 1-character name -> 400; unknown item and B's item id -> 404; Owner of B sees none of A's; `companyId` in query and body ignored |
| `R2`, `R3`, `AC3`, `AC12` | 1, 3 | Edit changes the values; the version name stays unique; no delete route; `GET audit?entityType=PriceVersion&entityId=<id>` as the Owner shows `CREATED` and `UPDATED` with old and new values |
| `R4`, `AC2` | 1, 4 | Versions `2025-26` (2025-04-01) and `2026-27` (2026-04-01) -> default `2026-27`; `2027-28` from 2027-04-01 not the default; two on the same date -> the later created; only a future version -> no default (a fixed `TimeProvider` date in the scratch check) |
| `R5`, `R6`, `AC5` | 4, 5 | Staff `GET` -> names, dates, MRP, sale price, empty purchase price (and the JSON has no purchase value); Staff `POST`, `PUT`, copy -> 403; anonymous -> 401 |
| `R5`, `AC6` | 5 | Super Admin with nothing entered -> empty, write -> 403 `core.company_required`; after entering A it works |
| `R8`, `AC8` | 3 | Copy `2025-26` to `2026-27` for one vendor's items: copied count, items without the source and with the target skipped and counted, nothing overwritten (SQL before and after) |
| `R9`, `AC7` | 4 | List search by item name, code, barcode; filters category, vendor, class; sort name and code; `pageSize=500` capped at 200; unknown `orderBy` ignored; each row shows its default version |
| `R15`, `R16`, `R17` | 1, 4 | Amounts `numeric(18,2)` (SQL); values with three decimals refused, not rounded; audit entries carry the Owner and company A |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The migration is additive; rollback: drop schema `price`. The default rule depends on the business date, so the check fixes the date with a test `TimeProvider` registration in scratch code that is not committed. If the owner answers Q2 with "latest created", `R4` and this plan change before it starts.
