# Plan price#05: Price directory for other modules

- **File:** `docs/plans/price/2026-11-15-05-price-directory.md`
- **Kind:** Backend
- **Spec:** [../../specs/price.md](../../specs/price.md) - rules covered: R18; AC13
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `price#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `docs/sales-order-invoice` (one branch `feature/sales` for company#06, price#05, stock#05, school#05, sales#01 and sales#03, the owner's instruction "do it all")

## 1. In one minute
After this plan the sales module can read an item's price versions and its default version on a date, and one version by id, without the purchase price and without touching the price tables.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Price` (capability, records), its implementation over the price store reusing `DefaultPrice` (R4).
Leaves out: endpoints, UI. Schema change: no.

## 3. Approach
The default rule of R4 already lives in `DefaultPrice`; the directory calls it with the given date, so it is not written twice.

## 4. Tasks (at most 8)
- [x] 1. Capability and records (R18).
- [x] 2. Implementation reusing `DefaultPrice` (R4).
- [x] 3. Build; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC13` | 1, 2 | Scratch call: book A with V26 (from 2026-01-01) and V27 (from 2027-01-01) -> versions `V27, V26` newest first, default on 2026-11-15 = V26; version by id -> its values; the Owner of B -> `{}` for A's version; no purchase price in the answer. **PASS** |

## 6. Not verified
Run by `check_sales.py` and `check_igst.py` (scratch, not committed; a temporary scratch controller for the capabilities was deleted before the commit) on the real `book_inventory`: 35/35 passed (the first IGST run failed only because an earlier check of the same script had changed the book's price; it was run again on fresh items and passed).

## 7. Risks and notes
None.
