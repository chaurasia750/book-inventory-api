# Plan item#03: Item directory capability (other modules read items)

- **File:** `docs/plans/item/2026-11-14-03-item-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R16; AC12
- **Owner:** abhishek
- **Status:** Done
- **Depends on:** `item#01` (Done)
- **UI plan:** none, it adds no screen or endpoint: it is a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14   **Completed:** 2026-11-14
- **Base branch:** `docs/price-management` (cut from it: item#03 builds on the item code of item#01 that this branch carries)

## 1. In one minute
After this plan another module (the price module first) can ask the item module three things without touching its tables: does this item exist in my company, which items match this search or filter, and what are the codes and names of these items. Nothing visible changes; the price module is the first user. You can see it working through the check script that calls the capability for the Owner of a company.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Item` (a capability `IItemDirectory` with its small result and filter records), `BookInventory.Infrastructure/Item` (its implementation on the existing store, no new table) and `BookInventory.Infrastructure/Item/ItemStore.cs` (the shared filter and two small methods), `docs/Code_Map.md`.
Leaves out: any endpoint, any change to an item, any UI.
Schema change: no.

## 3. Approach
`IItemDirectory` and its implementation both live in the item module's Application layer (no new Infrastructure class): the implementation is built on `IItemStore`, so the list, the id lookups and the directory share one filter (the store's filter was moved into one private method and two small store methods, `IdsAsync` and `RowsAsync`, were added), and on `IVendorDirectory` for the search by vendor name. It answers only for the session's company (P19). Methods: `GetAsync(id)` returns the item's id, code, name and active flag or null; `ReferencesAsync(ids)` returns the same by id; `PageAsync(ItemPageRequest)` returns one page of item ids for a search text (name, code, barcode, class, subject, vendor name) and a selection of vendor, category, class (ignoring case) and active-only, with the total; `AllIdsAsync(ItemSelection)` returns every matching id for a bulk operation, capped at 5000. It returns small records, never the aggregate.

## 4. Tasks (at most 8)
- [x] 1. `IItemDirectory`, `ItemReference` and `ItemSelection` in `BookInventory.Application/Item` (R16).
- [x] 2. The implementation in `BookInventory.Infrastructure/Item` over the company-filtered repository (R16).
- [x] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC12` | 1, 2 | A throw-away call (scratch code, not committed) inside the running API for the Owner of A: `GetAsync` of A's item -> id, code, name, active; of B's item id -> null; `ReferencesAsync` of two ids -> both; `PageAsync` by name, by barcode, by vendor and by class (any case) -> A's matching items only; `AllIdsAsync` with an empty selection -> all of A's items; for the Owner of B the same calls see only B's items **Observed (2026-11-14, real `book_inventory`, a scratch controller on a second API instance on port 7200, 17 checks, all pass; the controller was deleted afterwards):** A's `ITM-0001` -> id, code, name, active true; A's inactive `ITM-0002` -> active false; B's item id asked by A -> null; `ReferencesAsync` of A's two ids and one of B's -> only A's two; `PageAsync` by name, by barcode `8901234567890`, by vendor name `alpha`, by vendor id (count equals SQL), by class `5TH` matching `5th`, by category `PEN` with only active (count equals SQL), order `Code DESC` with page size 3 (`ITM-0016`, `ITM-0015`, `ITM-0014`); `AllIdsAsync` empty selection -> 16 ids = A's 16 items; B's call -> none of A's ids; B's search for `barcoded` finds only B's matches. Regression of the refactored list: `GET items` search by name, filters and search by vendor name still work. |

## 6. Not verified
- The 5000 cap of `AllIdsAsync` was not exercised with 5000+ items (only the `Take` in the code and 16 ids observed).
- No migration: this plan changes no schema.

## 7. Risks and notes
The check needs scratch code that calls the capability; the price plans exercise it end to end afterwards, which is the real proof. The 5000 cap protects one bulk request; a company with more items needs the selection narrowed.
