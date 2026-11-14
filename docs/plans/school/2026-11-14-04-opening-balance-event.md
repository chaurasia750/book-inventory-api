# Plan school#04: School tells the ledger about its opening balance

- **File:** `docs/plans/school/2026-11-14-04-opening-balance-event.md`
- **Kind:** Backend
- **Spec:** [../../specs/school.md](../../specs/school.md) - rules covered: R17; AC13
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `school#01` (Done)
- **UI plan:** none, nothing visible changes: the school raises one more event
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q1-Q11 of the finance spec answered one by one, all with the recommended answers)
- **Created:** 2026-11-14

## 1. In one minute
After this plan a school raises `SchoolOpeningBalanceChanged` (old and new amount) when it is created with an opening balance or the balance is edited, so the ledger (finance#02) can post it without the school ever writing the ledger. Nothing visible changes.

## 2. Touches / leaves out
Touches: `BookInventory.Domain/School` (the event record; `School.Create` and `School.Edit` raise it when the amount changes), `docs/Code_Map.md` if a line is needed.
Leaves out: the ledger reaction (finance#02), any endpoint, any UI, any schema change.
Schema change: no.

## 3. Approach
The root already owns the opening balance (R7): its `Apply` compares the old and new value and raises the event only when they differ (creation counts from 0). The event carries ids and amounts only.

## 4. Tasks (at most 8)
- [ ] 1. `SchoolOpeningBalanceChanged(SchoolId, OldAmount, NewAmount)` and the raise in the root (R17).
- [ ] 2. Build at 0 warnings, 0 errors.
- [ ] 3. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC13` | 1 | Through finance#02's handler (or a scratch handler, not committed): create with 5000.00 -> one event 0 -> 5000.00; edit to 4500.00 -> one event; edit keeping 4500.00 -> none; create with 0 -> none |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
A domain event with no handler is harmless; finance#02 adds the handler.
