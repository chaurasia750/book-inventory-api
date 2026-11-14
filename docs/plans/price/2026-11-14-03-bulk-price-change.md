# Plan price#03: Bulk price change

- **File:** `docs/plans/price/2026-11-14-03-bulk-price-change.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R10-R14, R17; AC9-AC11
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `price#01`
- **UI plan:** price#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `feature/price-01` (cut from it: price#03 builds on the price versions of price#01)

## 1. In one minute
After this plan an Owner can pick a group of items (a vendor, a category, a class or everything), say "raise MRP by 10 percent and lower the purchase price by 5 rupees", look at a preview of every old and new price, and then apply it. Try it: `POST /api/v1/prices/bulk/preview` with `{versionName: "2026-27", selection: {category: "BOOK"}, mrp: {kind: "PERCENT", direction: "INCREASE", value: 10}}` and read the preview; the same body to `POST /api/v1/prices/bulk/apply` changes those versions.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Price` (the change calculation as a value object), `BookInventory.Application/Price` (preview and apply use cases, and the company guard that the version service now shares), `BookInventory.Infrastructure/Price/PriceStore.cs` (one tracked read of the named versions), `BookInventory.Api/Controllers/Price/PricesController.cs` (two endpoints), `docs/Code_Map.md`.
Leaves out: Excel upload, copy (done in price#01), any UI (price#04), creating new versions in bulk (Q3).
Schema change: no.

## 3. Approach
The calculation lives in one value object, `PriceChange` (kind, direction, value) with `Apply(decimal current)`: it works in exact `decimal`, applies a percentage to the current value of that same field, rounds once at the end half away from zero at two decimals (the owner's answer to Q1), and reports "below zero" or "overflow" instead of storing a wrong value. Preview and apply share one function that turns a selection and the changes into a list of (version, old values, new values, skipped reason), so the preview can never differ from what apply does; apply recomputes it on the server and edits each version through its own root in one request transaction (decision D4). The selection is turned into item ids by the item directory (item#03), capped at 5000 items; a selection that matches nothing is `400`.

## 4. Tasks (at most 8)
- [x] 1. Domain: `PriceChange` (percent and amount, increase and decrease, the rounding of Q1, below-zero and overflow outcomes) and the `PriceVersion` edit it uses (R11, R17).
- [x] 2. Use case `PreviewBulkChange`: selection to items, versions of the named version, old and new values, skipped reasons, totals, paged (R10, R12).
- [x] 3. Use case `ApplyBulkChange`: the same computation, then the edits in one transaction; `400` for an empty selection (R13, R14).
- [x] 4. Controller endpoints `POST prices/bulk/preview` and `POST prices/bulk/apply` (Owner and entered Super Admin).
- [x] 5. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 6. Verify every row of section 5 on the real local database, with the money rows worked out independently first, and record inputs and observed outputs.

## 5. Spec coverage
Money rows were worked out independently **before** any request (halves away from zero at two decimals, owner's answer to Q1); the observed values are from the real `book_inventory` on a scratch API instance on port 7200 (23 checks, all passed on the first run).

| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `R12`, `AC9` | 2, 4 | Preview for one category with MRP +10 percent and purchase -5 rupees: old and new per item, skipped with reasons, totals; SQL shows no row changed **Observed:** previews selected by class (`bt1` found `BT1`: the selection ignores case) and by category `OTHER`; each row has the old and the new prices or the skip reason; with page size 2 the pages 1 and 2 hold different items and the same totals (`itemsSelected 8, changed 4, skippedNoVersion 3, skippedOverflow 1`); an md5 of all price versions (ids, three amounts, `updated_at`) was identical before and after the previews. |
| `R13`, `R14`, `AC10` | 3, 4 | Apply with the same body: exactly the previewed items change to the previewed values, one history entry each, an item that would go below zero is skipped, an empty selection -> 400; documents untouched **Observed:** each apply stored exactly what its preview showed (rows C1, C2, C5 below); C3, C4, C6 changed nothing; the changed version has an `UPDATED` entry of company ABC in `core.audit_entry` and the Owner reads it through `GET audit`; a selection that matches no item -> 400 `price.nothing_selected`; no field to change -> 400 `price.invalid_change`; the Owner of B (nothing matches in B) -> 400 and A's price stayed `2.53/6.67/3.47`. |
| `R11`, `R17`, `AC11` C1 | 1 | Expected first: MRP 2.50 +1 percent = 2.525 -> **2.53**; sale 10.00 -33.33 percent = 10.00 - 3.333 = 6.667 -> **6.67**; purchase 3.50 -1 percent = 3.50 - 0.035 = 3.465 -> **3.47** (to the even digit it would be 3.46) **Observed:** preview `2.53 / 6.67 / 3.47`, apply stored `2.53/6.67/3.47`. |
| `R11`, `R17`, `AC11` C2 | 1 | Expected first: 125.00 +10 percent = **137.50**; 100.00 -33.33 percent = **66.67**; 99.99 +0 percent = **99.99** **Observed:** preview and stored `137.50/66.67/99.99`. |
| `R11`, `AC11` C3 | 1 | Expected first: 10.00 minus 10.01 rupees = -0.01 -> skipped below zero, the item unchanged **Observed:** `skipped BELOW_ZERO`, no new values; after apply `changed 0, skippedBelowZero 1` and the stored `10.00/10.00/10.00`. |
| `R11`, `AC11` C4 | 1 | Expected first: `9999999999999999.99` +1 percent is above `numeric(18,2)` -> skipped overflow, the value stays **Observed:** `skipped OVERFLOW`; stored `9999999999999999.99/1.00/1.00` after apply. |
| `R11`, `AC11` C5 | 1 | Expected first: 0.05 +50 percent = 0.075 -> **0.08**; 5.00 + 0.01 rupees = **5.01**; 8.00 - 8.00 rupees = **0.00** (exactly zero is allowed) **Observed:** preview and stored `0.08/5.01/0.00`. |
| `R12`, `AC9` C6 | 2 | An item without the named version is listed as skipped `NO_VERSION` and nothing is created **Observed:** `skipped NO_VERSION`, `skippedNoVersion 1`, no row created. |
| `R11` | 1 | A value with three decimals (`1.005` percent) and a negative value -> 400 `price.invalid_change` **Observed:** both 400 `price.invalid_change`. |
| `R5`, `R6` | 4 | Staff on both endpoints -> 403; anonymous -> 401; the Owner of B cannot change A's versions **Observed:** Staff preview and apply 403, anonymous 401, Super Admin with no company 403 `core.company_required`; the Owner of B cannot change A's. |

## 6. Not verified
- That an apply is all or nothing when one edit fails half way was not exercised (it relies on the request transaction; no failure can be provoked from outside once the plan is computed).
- Selections by vendor, by category and by class together, and the 5000-item cap, were not exercised with large data (class, category and a small whole-list selection were).
- Percentages with two decimals other than `33.33` and `1` were not tried; the calculation is exact decimal arithmetic for all of them.
- No migration: this plan changes no schema.
- The check left the 6 `P03 BT...` test items (class `BT1` to `BT6`) with their `BULK-T` versions in the dev database (B27 test data).

## 7. Risks and notes
Rounding (Q1: half away from zero at two decimals) and what a bulk change edits (Q3: the named version) were answered by the owner on 2026-11-14. A bulk apply over thousands of items is one long transaction; the 5000 cap keeps it bounded.
