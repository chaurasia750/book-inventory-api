# Plan purchase#05: Dashboard figures of vendor returns

- **File:** `docs/plans/purchase/2026-11-16-05-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R8 (the vendor-return part); AC5
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories)
- **Depends on:** `purchase#01` (Done)
- **UI plan:** none, a read capability for the report module
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q4 and the dashboard choices answered one by one; "all approve")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the report module can ask purchase for the number and the total of live vendor returns in a date range.

## 2. Touches / leaves out
Touches: `Application/Purchase` (a figures capability) and `Infrastructure/Purchase` (one grouped query), `docs/Code_Map.md`. Leaves out: the screens.

## 3. Approach
`IPurchaseFigures.ReturnsAsync(range)`: count and total of `ACTIVE` vendor returns by return date.

## 4. Tasks (at most 8)
- [x] 1. The capability and its answer record (R8).
- [x] 2. The grouped query.
- [x] 3. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 4. Verify AC5 (the vendor-return part) on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R8`, `AC5` | 1-4 | a vendor return of 360 inside the window and one before it: count 1, total 360; a cancelled one left out ; the vendor returns of the window show 5 / 19,447.00 on the dev data; a cancelled one left out and one before the window were not tried |

## 6. Not verified
Build: `dotnet build src/BookInventory.Api` to a scratch output (the owner's own API holds the default `bin`) -> 0 warnings, 0 errors.

Scratch check on the real `book_inventory` (scratch API on 7900, owner login): 31 checks, all passed; the dashboard was read before and after each step and the deltas compared with figures worked out first.

NOT verified: the figure against a hand sum of the vendor returns list, the cancelled and out-of-window cases.

## 7. Risks and notes
No migration.
