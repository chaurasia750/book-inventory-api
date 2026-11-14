# Plan item#03: Item directory capability (other modules read items)

- **File:** `docs/plans/item/2026-11-14-03-item-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R16; AC12
- **Owner:** abhishek
- **Status:** Draft
- **Depends on:** `item#01` (Done)
- **UI plan:** none, it adds no screen or endpoint: it is a read capability for other modules
- **Approved by / on:** pending (rule R16 is Draft; it exists for the price spec)
- **Created:** 2026-11-14

## 1. In one minute
After this plan another module (the price module first) can ask the item module three things without touching its tables: does this item exist in my company, which items match this search or filter, and what are the codes and names of these items. Nothing visible changes; the price module is the first user. You can see it working through the check script that calls the capability for the Owner of a company.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Item` (a capability `IItemDirectory` with its small result and filter records), `BookInventory.Infrastructure/Item` (its implementation on the existing store, no new table), `docs/Code_Map.md`.
Leaves out: any endpoint, any change to an item, any UI.
Schema change: no.

## 3. Approach
`IItemDirectory` lives in the item module's Application layer and is implemented in its Infrastructure over the same company-filtered repository as `ItemStore`, so it answers only for the session's company (P19). Methods: `GetAsync(id)` returns the item's id, code, name and active flag or null; `SummariesAsync(ids)` returns id, code and name by id; `IdsMatchingAsync(ItemSelection, search, page)` returns one page of item ids for a search text over name, code and barcode and a selection of vendor, category, class (ignoring case) and active-only, plus the total; `AllIdsAsync(ItemSelection)` returns every matching id for a bulk operation, capped at 5000. It returns small records, never the aggregate.

## 4. Tasks (at most 8)
- [ ] 1. `IItemDirectory`, `ItemReference` and `ItemSelection` in `BookInventory.Application/Item` (R16).
- [ ] 2. The implementation in `BookInventory.Infrastructure/Item` over the company-filtered repository (R16).
- [ ] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC12` | 1, 2 | A throw-away call (scratch code, not committed) inside the running API for the Owner of A: `GetAsync` of A's item -> id, code, name, active; of B's item id -> null; `SummariesAsync` of two ids -> both; `IdsMatchingAsync` by name, by barcode, by vendor and by class (any case) -> A's matching items only; `AllIdsAsync` with an empty selection -> all of A's items; for the Owner of B the same calls see only B's items |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The check needs scratch code that calls the capability; the price plans exercise it end to end afterwards, which is the real proof. The 5000 cap protects one bulk request; a company with more items needs the selection narrowed.
