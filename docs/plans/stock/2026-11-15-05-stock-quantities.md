# Plan stock#05: Stock quantities for other modules

- **File:** `docs/plans/stock/2026-11-15-05-stock-quantities.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R20; AC11
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `stock#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the sales module can read the Main, Damaged and Sample quantities of the items on an order (for the yellow low-stock warning).

## 2. Touches / leaves out
Touches: `BookInventory.Application/Stock` (capability over the existing `TotalsAsync`).
Leaves out: endpoints, UI. Schema change: no.

## 3. Approach
A thin capability over the store's totals, at most 500 ids, zero for items without a card.

## 4. Tasks (at most 8)
- [ ] 1. Capability (R20).
- [ ] 2. Build; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC11` | 1 | Throw-away call: a book with stock -> its quantities; a book without a card -> 0; B's item -> 0 / absent |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
None.
