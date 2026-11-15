# Plan company#06: Company tax profile for other modules

- **File:** `docs/plans/company/2026-11-15-06-company-tax-profile.md`
- **Kind:** Backend
- **Spec:** [../../specs/company.md](../../specs/company.md) - rules covered: R16; AC17
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `company#01` (Done)
- **UI plan:** none, a read capability for other modules
- **Approved by / on:** abhishek / 2026-11-15 (chat: sales Q1-Q14 answered one by one)
- **Created:** 2026-11-15
- **Base branch:** `docs/sales-order-invoice` (one branch `feature/sales` for company#06, price#05, stock#05, school#05, sales#01 and sales#03, the owner's instruction "do it all")

## 1. In one minute
After this plan the sales module can ask the company module for the state and GSTIN of the session's company (to choose CGST+SGST or IGST) without touching its tables. Nothing visible changes.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Company` (a small capability and its record), its implementation over the company store.
Leaves out: any endpoint or UI. Schema change: no.

## 3. Approach
Same shape as the other directories: one method returning a small record for the company of the session (null without a company).

## 4. Tasks (at most 8)
- [x] 1. Capability and implementation (R16).
- [x] 2. Build at 0 warnings, 0 errors; verify section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC17` | 1 | Scratch call as the Owner of A: `{state: null, gstin: null}` (A has none filled); with A's state set to Delhi the sales module chose IGST (sales AC2). **PASS** |

## 6. Not verified
Run by `check_sales.py` and `check_igst.py` (scratch, not committed; a temporary scratch controller for the capabilities was deleted before the commit) on the real `book_inventory`: 35/35 passed (the first IGST run failed only because an earlier check of the same script had changed the book's price; it was run again on fresh items and passed).

## 7. Risks and notes
None.
