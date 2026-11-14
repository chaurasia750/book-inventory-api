# Plan price#03: Bulk price change

- **File:** `docs/plans/price/2026-11-14-03-bulk-price-change.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R10-R14, R17; AC9-AC11
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `price#01`
- **UI plan:** price#04
- **Approved by / on:** pending (R10-R14 are Draft; Q1 rounding and Q3 what a bulk change edits are open)
- **Created:** 2026-11-14

## 1. In one minute
After this plan an Owner can pick a group of items (a vendor, a category, a class or everything), say "raise MRP by 10 percent and lower the purchase price by 5 rupees", look at a preview of every old and new price, and then apply it. Try it: `POST /api/v1/prices/bulk/preview` with `{versionName: "2026-27", selection: {category: "BOOK"}, mrp: {kind: "PERCENT", direction: "INCREASE", value: 10}}` and read the preview; the same body to `POST /api/v1/prices/bulk/apply` changes those versions.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Price` (the change calculation as a value object), `BookInventory.Application/Price` (preview and apply use cases), `BookInventory.Api/Controllers/Price/PricesController.cs` (two endpoints), `docs/Code_Map.md`.
Leaves out: Excel upload, copy (done in price#01), any UI (price#04), creating new versions in bulk (Q3).
Schema change: no.

## 3. Approach
The calculation lives in one value object, `PriceChange` (kind, direction, value) with `Apply(decimal current)`: it works in exact `decimal`, applies a percentage to the current value of that same field, rounds once at the end with the mode the owner answered for Q1 (written in the spec), and reports "below zero" or "overflow" instead of storing a wrong value. Preview and apply share one function that turns a selection and the changes into a list of (version, old values, new values, skipped reason), so the preview can never differ from what apply does; apply recomputes it on the server and edits each version through its own root in one request transaction (decision D4). The selection is turned into item ids by the item directory (item#03), capped at 5000 items; a selection that matches nothing is `400`.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `PriceChange` (percent and amount, increase and decrease, the rounding of Q1, below-zero and overflow outcomes) and the `PriceVersion` edit it uses (R11, R17).
- [ ] 2. Use case `PreviewBulkChange`: selection to items, versions of the named version, old and new values, skipped reasons, totals, paged (R10, R12).
- [ ] 3. Use case `ApplyBulkChange`: the same computation, then the edits in one transaction; `400` for an empty selection (R13, R14).
- [ ] 4. Controller endpoints `POST prices/bulk/preview` and `POST prices/bulk/apply` (Owner and entered Super Admin).
- [ ] 5. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 6. Verify every row of section 5 on the real local database, with the money rows worked out independently first, and record inputs and observed outputs.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `R12`, `AC9` | 2, 4 | Preview for one category with MRP +10 percent and purchase -5 rupees: old and new per item, skipped with reasons, totals; SQL shows no row changed |
| `R13`, `R14`, `AC10` | 3, 4 | Apply with the same body: exactly the previewed items change to the previewed values, one history entry each (`GET audit`), an item that would go below zero is skipped, an empty selection -> 400; documents untouched (none exist) |
| `R11`, `R17`, `AC11` | 1 | Worked out first and then observed: 2.50 +1 percent -> 2.53 (or 2.52 per Q1); 100.00 -33.33 percent -> 66.67; 125.00 +10 percent -> 137.50; 10.00 minus 10.01 -> skipped below zero; 99.99 +0 percent -> 99.99; the maximum `numeric(18,2)` value +1 percent -> refused as overflow; amounts with three decimals in the request -> 400 |
| `R5`, `R6` | 4 | Staff on both endpoints -> 403; anonymous -> 401; the Owner of B cannot change A's versions |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Rounding (Q1) and what a bulk change edits (Q3) are business decisions: this plan does not start before the owner answers them. A bulk apply over thousands of items is one long transaction; the 5000 cap keeps it bounded.
