# Plan stock#05: Stock quantities for other modules

- **File:** `docs/plans/stock/2026-11-15-05-stock-quantities.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R20; AC11
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `stock#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `docs/sales-order-invoice` (one branch `feature/sales` for company#06, price#05, stock#05, school#05, sales#01 and sales#03, the owner's instruction "do it all")

## 1. In one minute
After this plan the sales module can read the Main, Damaged and Sample quantities of the items on an order (for the yellow low-stock warning).

## 2. Touches / leaves out
Touches: `BookInventory.Application/Stock` (capability over the existing `TotalsAsync`).
Leaves out: endpoints, UI. Schema change: no.

## 3. Approach
A thin capability over the store's totals, at most 500 ids, zero for items without a card.

## 4. Tasks (at most 8)
- [x] 1. Capability (R20).
- [x] 2. Build; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC11` | 1 | Scratch call: book A after an opening of 12 -> Main 12; copy B without a card -> 0, 0, 0. **PASS** |

## 6. Not verified
Run by `check_sales.py` and `check_igst.py` (scratch, not committed; a temporary scratch controller for the capabilities was deleted before the commit) on the real `book_inventory`: 35/35 passed (the first IGST run failed only because an earlier check of the same script had changed the book's price; it was run again on fresh items and passed).

## 7. Risks and notes
None.
