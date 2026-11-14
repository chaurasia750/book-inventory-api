# Plan vendor#04: Vendor tells the ledger about its opening balance

- **File:** `docs/plans/vendor/2026-11-14-04-opening-balance-event.md`
- **Kind:** Backend
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R16; AC14
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `vendor#01` (Done)
- **UI plan:** none, nothing visible changes: the vendor raises one more event
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14
- **Base branch:** `docs/finance-ledger` (one branch `feature/finance` for platform#08, school#03, school#04, vendor#04 and finance#01-#03, the owner's instruction "all plan done kro")

## 1. In one minute
After this plan a vendor raises `VendorOpeningBalanceChanged` (old and new amount) when it is created with an opening balance or the balance is edited, so the ledger (finance#02) can post it without the vendor ever writing the ledger. Nothing visible changes.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Vendor` (the event record; `Vendor.Create` and `Vendor.Edit` raise it when the amount changes).
Leaves out: the ledger reaction (finance#02), any endpoint, any UI, any schema change.
Schema change: no.

## 3. Approach
The same as school#04: the root compares the old and new opening balance (R6) and raises the event only when they differ (creation counts from 0).

## 4. Tasks (at most 8)
- [x] 1. `VendorOpeningBalanceChanged(VendorId, OldAmount, NewAmount)` and the raise in the root (R16).
- [x] 2. Build at 0 warnings, 0 errors.
- [x] 3. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC14` | 1 | Through finance#02's handler: create with 2000.00 -> Dr OPENING_BALANCE / Cr VENDOR_PAYABLE 2000.00, balance 2000.00; edit to 2500.00 -> balance 2500.00. **PASS** |

## 6. Not verified
Run by `check_finance.py` (scratch, not committed; a temporary scratch controller and a throw-away failing handler were deleted before the commit): 28/28 passed on the real `book_inventory`. The event also carries the vendor's code.

## 7. Risks and notes
A domain event with no handler is harmless; finance#02 adds the handler.
