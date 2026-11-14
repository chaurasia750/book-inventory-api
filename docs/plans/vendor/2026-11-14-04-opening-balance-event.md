# Plan vendor#04: Vendor tells the ledger about its opening balance

- **File:** `docs/plans/vendor/2026-11-14-04-opening-balance-event.md`
- **Kind:** Backend
- **Spec:** [../../specs/vendor.md](../../specs/vendor.md) - rules covered: R16; AC14
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `vendor#01` (Done)
- **UI plan:** none, nothing visible changes: the vendor raises one more event
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan a vendor raises `VendorOpeningBalanceChanged` (old and new amount) when it is created with an opening balance or the balance is edited, so the ledger (finance#02) can post it without the vendor ever writing the ledger. Nothing visible changes.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/Vendor` (the event record; `Vendor.Create` and `Vendor.Edit` raise it when the amount changes).
Leaves out: the ledger reaction (finance#02), any endpoint, any UI, any schema change.
Schema change: no.

## 3. Approach
The same as school#04: the root compares the old and new opening balance (R6) and raises the event only when they differ (creation counts from 0).

## 4. Tasks (at most 8)
- [ ] 1. `VendorOpeningBalanceChanged(VendorId, OldAmount, NewAmount)` and the raise in the root (R16).
- [ ] 2. Build at 0 warnings, 0 errors.
- [ ] 3. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R16`, `AC14` | 1 | Through finance#02's handler (or a scratch handler, not committed): create with 2000.00 -> one event 0 -> 2000.00; edit to 2500.00 -> one event; edit keeping it -> none; create with 0 -> none |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
A domain event with no handler is harmless; finance#02 adds the handler.
