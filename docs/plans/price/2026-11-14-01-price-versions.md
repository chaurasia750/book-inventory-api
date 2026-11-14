# Plan price#01: Price versions

- **File:** `docs/plans/price/2026-11-14-01-price-versions.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R1-R9, R15-R17; AC1-AC8, AC12
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `platform#01` (Done), `item#03` (Done)
- **UI plan:** price#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `feature/item-03` (cut from it: price#01 needs the item directory of item#03)

## 1. In one minute
After this plan an Owner can add and edit price versions of an item (MRP, sale price, purchase price, effective date), see which version is the default, copy a whole version to a new one for the new year, and see the price list; Staff can read prices without the purchase price; no company sees another's prices. Try it: as an Owner `POST /api/v1/items/{itemId}/prices` with `{versionName: "2026-27", effectiveFrom: "2026-04-01", mrpAmount: 125, saleAmount: 112.5, purchaseAmount: 100}`, then `GET /api/v1/items/{itemId}/prices` shows it as the default.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Price`, `BookInventory.Application/Price`, `BookInventory.Infrastructure/Price` and `Persistence` (schema `price`, migration), `BookInventory.Api/Controllers/Price/PricesController.cs`, `CLAUDE.project.md` (the module list gets `price`, in its own commit), `docs/Code_Map.md`.
Leaves out: bulk price change (price#03), any UI (price#02), Excel upload, the purchase warning, picking a version on a document.
Schema change: yes (new schema `price`, additive).

## 3. Approach
`PriceVersion` derives from `CompanyOwnedRoot`; its item is a plain id, checked through the item directory (item#03). `PriceRules` (name, amounts with at most two decimals, not negative) and the default-version rule (latest effective date not after the business date, ties by creation) are written once, the default as a small domain function over a list of versions so the list query and the single-item read use the same rule. "Today" comes from `TimeProvider` converted to `Asia/Kolkata`. Staff reads are projected without the purchase price in the query service (the Owner's projection is a separate record, so the amount cannot leak by accident). The price list asks the item directory for a page of item ids (search, filters, sort) and then loads those items' versions in one query to pick each default. Copy is one request transaction over many versions (decision D4). History is not built here: it is the audit trail of `core` (R15). A write with no company in the session (a Super Admin who has not entered one) is refused first with `403 core.company_required` (P20), before the item is looked up, so it is not mistaken for an unknown item.

## 4. Tasks (at most 8)
- [x] 1. Domain: `PriceVersion` (add, edit), `PriceRules`, the default-version rule, events, message codes (R2, R3, R4, R17).
- [x] 2. Persistence: configuration with the per-company unique indexes, schema `price`, migration; add `price` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [x] 3. Use cases `AddVersion`, `EditVersion`, `CopyVersion` (R1, R2, R3, R8).
- [x] 4. Query service: an item's versions with the default marked, Staff without purchase price, the price list with search, filters and sort through the item directory (R4, R5, R9).
- [x] 5. `PricesController` with the roles of the API table (R5, R6).
- [x] 6. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 7. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R1`, `AC1`, `AC4` | 3, 5 | Owner of A adds `2026-27` to A's item -> 201, `company_id` A (SQL); same name in another case -> 409; negative value, three decimals, 1-character name -> 400; unknown item and B's item id -> 404; Owner of B sees none of A's; `companyId` in query and body ignored **Observed (2026-11-14, real `book_inventory`, 41 checks, scratch API instance on port 7200; the first run showed 40 of 41 because one expected count was wrong, see section 6):** `POST items/{X}/prices` `{2026-27, 2026-04-01, 125, 112.5, 100}` -> 201 with those values; SQL: the row's company is ABC; `2026-27`, `2026-27 ` and `2026-27` in upper case -> 409 `price.name_taken`; MRP -1 -> 400 `price.invalid_amount`; sale 10.125 -> 400 (not rounded); purchase 10000000000000000 -> 400; name `N` -> 400 `price.invalid_name`; unknown item and the Children item asked by ABC -> 404 `price.item_not_found`; the Children Owner reading ABC's item prices (with `companyId` in the query) -> 404 and the Children list holds none of ABC's items. |
| `R2`, `R3`, `AC3`, `AC12` | 1, 3 | Edit changes the values; the version name stays unique; no delete route; `GET audit?entityType=PriceVersion&entityId=<id>` as the Owner shows `CREATED` and `UPDATED` with old and new values **Observed:** the edit changed MRP 125 -> 130, sale 112.5 -> 115.25, purchase 100 -> 101 in place; renaming to the item's other version name -> 409; editing through another item's route -> 404 `price.version_not_found`; `DELETE` -> 405; the version has a `CREATED` and an `UPDATED` entry (company ABC) in `core.audit_entry` and the Owner reads both through `GET audit` (2 items). |
| `R4`, `AC2` | 1, 4 | Versions `2025-26` and `2026-27` -> default `2026-27`; a future version not the default; two on the same date -> the later created; only a future version -> no default **Observed (today in India = 2026-11-14):** versions from today-400 days, 2026-04-01 and today+60 days -> default is `2026-27`, list newest first `2027-28`, `2026-27`, `2025-26`; two versions both from today-10 days (`A-old` then `B-new`) -> `B-new` is the default; an item with only a version from today+30 days -> no default. |
| `R5`, `R6`, `AC5` | 4, 5 | Staff `GET` -> names, dates, MRP, sale price, empty purchase price; Staff `POST`, `PUT`, copy -> 403; anonymous -> 401 **Observed:** Staff read -> `mrpAmount` 50, `saleAmount` 45, `purchaseAmount` null and the raw JSON holds no `30`; the Staff list shows no purchase value; Staff `POST`, `PUT`, copy -> 403; anonymous -> 401. |
| `R5`, `AC6` | 5 | Super Admin with nothing entered -> empty, write -> 403 `core.company_required`; after entering A it works **Observed:** empty list and `POST` -> 403 `core.company_required`; after entering ABC the read -> 200 and `POST` -> 201. |
| `R8`, `AC8` | 3 | Copy `2025-26` to `2026-27` for one vendor's items: copied count, items without the source and with the target skipped and counted, nothing overwritten **Observed (SQL checked):** the vendor has 5 items in ABC; copy -> `{itemsSelected 5, copied 2, skippedNoSourceVersion 2, skippedAlreadyHasTarget 1}`: the two sources became `2026-27` with the same `10.00/9.00/8.00` and `30.00/27.00/25.00`, the item that already had `2026-27` (`99.00/98.00/97.00`) was not overwritten, the two without a source got nothing; the copies carry effective date 2026-04-01; copying again -> `copied 0`, `skippedAlreadyHasTarget 3`. |
| `R9`, `AC7` | 4 | List search by item name, code, barcode; filters category, vendor, class; sort name and code; `pageSize=500` capped at 200; unknown `orderBy` ignored; each row shows its default version **Observed:** each row shows its default (X `2026-27`, Y `B-new`, Z none); search by item name found 4; vendor filter 4 of the P01 items; category filter 8; `ItemName DESC` and `ItemCode` sorted; `pageSize=500` -> 200; unknown `orderBy` -> 200. Search by barcode and the class filter go through the same item directory that item#03 verified. |
| `R15`, `R16`, `R17` | 1, 4 | Amounts `numeric(18,2)` (SQL); values with three decimals refused, not rounded; audit entries carry the Owner and company A **Observed:** the three amount columns are `numeric(18,2)`; the 3-decimal and above-maximum values were refused; the audit entries are company ABC's. |

## 6. Not verified
- The first run of the check printed 40 of 41: its expected counts for the copy assumed 4 items for the vendor, but ABC's vendor also had an older item from the item checks (5 items). SQL confirmed the 5, and the observed numbers are the correct ones (2 copied, 2 without a source, 1 already there). The check's expectation was wrong, not the code.
- The migration `Price_Initial` (new schema `price`, table `price.price_version`, two indexes; additive) was applied to the local development database `book_inventory` by the app at its start, on the owner's approval of this plan (its SQL was shown in the session; target `book_inventory` on `localhost:5432`). It is **not** applied to any shared database; the owner applies it there.
- The business date comes from the real clock (`Asia/Kolkata`); a day-boundary case (just before and after midnight India time) was not exercised.
- Search by barcode and the class filter of the price list were not exercised separately here (item#03 verified them in the directory).
- The check left test versions on 11 `P01 ...` items in the dev database (B27 test data).

## 7. Risks and notes
The migration is additive; rollback: drop schema `price`. The default rule depends on the business date, so the check fixes the date with a test `TimeProvider` registration in scratch code that is not committed. If the owner answers Q2 with "latest created", `R4` and this plan change before it starts.
