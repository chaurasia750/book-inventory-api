# Plan item#04: Item directory tells whether an item is stock managed

- **File:** `docs/plans/item/2026-11-14-04-item-directory-stock-flag.md`
- **Kind:** Backend
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R17; AC13
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `item#03` (Done)
- **UI plan:** none, it adds no screen or endpoint: it widens a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14
- **Base branch:** `docs/stock-ledger` (implemented on the one branch `feature/stock` with the stock plans, the owner's instruction "ek hi branch pe")

## 1. In one minute
After this plan the item directory also says whether an item is stock managed, and can list only the stock-managed items of a selection, so the stock module never keeps its own copy of that flag. Nothing visible changes; the stock module is the first user.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Item` (`ItemReference` gets `IsStockManaged`; `ItemSelection` gets an optional stock-managed filter; `ItemDirectory` maps both), `BookInventory.Application/Item/IItemStore.cs` and `BookInventory.Infrastructure/Item/ItemStore.cs` only if the row lacks the flag (it already carries it), `docs/Code_Map.md`.
Leaves out: any endpoint, any UI, any change to how an item is stored.
Schema change: no.

## 3. Approach
The item row the directory reads already holds `IsStockManaged`; the reference record and the selection gain it, and the directory passes the filter into the one shared item filter (R12), so nothing is written twice. Existing callers (the price module) keep compiling: the new selection field is optional and defaults to "any".

## 4. Tasks (at most 8)
- [x] 1. `ItemReference.IsStockManaged` and `ItemSelection.StockManaged` (optional) (R17).
- [x] 2. `ItemDirectory` maps both through the shared filter; price callers unchanged (R17).
- [x] 3. Build at 0 warnings, 0 errors; `docs/Code_Map.md`.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC13` | 1, 2 | Scratch controller (deleted, not committed), Owner of A, real `book_inventory`: book `ITM-0032` -> `isStockManaged: true`, pen `ITM-0033` -> `false`; a page with `StockManaged = true` -> total 18, SQL count of live stock-managed items of A = 18; regression `GET /prices?pageSize=5` -> 200. Also used by stock#01: the stock list never lists the pen, its item stock -> 400 `stock.not_stock_managed`. **PASS** |

## 6. Not verified
Nothing.

## 7. Risks and notes
A small widening of a shared capability; the price module's calls are re-run as a regression.
