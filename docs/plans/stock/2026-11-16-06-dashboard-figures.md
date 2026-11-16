# Plan stock#06: Dashboard figures of stock

- **File:** `docs/plans/stock/2026-11-16-06-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R10 (the stock side); AC7
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories)
- **Depends on:** `stock#05` (Done, quantities), `price#06` (the purchase price, used by the report module)
- **UI plan:** none, a read capability for the report module
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q4 and the dashboard choices answered one by one; "all approve")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the report module can ask stock for the number of stock-managed items at or below zero Main stock, those below zero, the Damaged pieces, and the Main quantity of every item above zero (to value it).

## 2. Touches / leaves out
Touches: `Application/Stock` (a figures capability) and `Infrastructure/Stock` (aggregate queries over the item stock), `docs/Code_Map.md`. Leaves out: the stock screens, the price lookup (the report module multiplies with price R19).

## 3. Approach
`IStockFigures` with `CountsAsync()` (low, below zero, damaged pieces) and `MainQuantitiesAsync(page)` (item id and Main quantity above zero, paged by 500 so the caller can price them with price R19).

## 4. Tasks (at most 8)
- [x] 1. The capability and its answer records (R10).
- [x] 2. The counts query (zero or below, below zero, damaged pieces).
- [x] 3. The paged Main quantities above zero.
- [x] 4. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 5. Verify AC7 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R10`, `AC7` | 1-3 | items at 0 and -3 are counted (the -3 one also below zero); damaged pieces add up; the quantities above zero are listed ; items at or below zero equal the stock list's `atOrBelowZeroOnly` count (65 = 65); the Main quantities above zero are priced by the report module (stock value 41.00 on the dev data, no item without a price) |

## 6. Not verified
Build: `dotnet build src/BookInventory.Api` to a scratch output (the owner's own API holds the default `bin`) -> 0 warnings, 0 errors.

Scratch check on the real `book_inventory` (scratch API on 7900, owner login): 31 checks, all passed; the dashboard was read before and after each step and the deltas compared with figures worked out first.

NOT verified: the below-zero and damaged counts against hand figures (only at-or-below-zero was compared), the value against a hand calculation, a company with more items than the item directory's id cap.

## 7. Risks and notes
No migration.
