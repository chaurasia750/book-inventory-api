# Plan finance#05: Balances of a list of schools or vendors

- **File:** `docs/plans/finance/2026-11-15-05-party-balances.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R17; AC9
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Depends on:** `finance#01` (Done)
- **UI plan:** finance#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: the profile questions - profile panel with tabs, balance column in the lists, the same for vendors)
- **Created:** 2026-11-15
- **Base branch:** `docs/item-price-stock-menu` (the owner's menu and profile changes on one branch)

## 1. In one minute
After this plan the school and vendor lists can show each row's balance: one request gives the balances of the ids on the page. Try it: `GET /api/v1/ledger/balances/schools?ids=<id1>&ids=<id2>`.

## 2. Touches / leaves out
Touches: `BookInventory.Application/Finance` (a small query use case over `ILedgerBalances` and the directories), `BookInventory.Api/Controllers/Finance` (two GET routes), `docs/Code_Map.md` if needed.
Leaves out: any schema change, the UI (finance#06).
Schema change: no.

## 3. Approach
The ids are first kept to the session's company through the school or vendor directory (another company's or unknown ids drop out), then `ILedgerBalances.BalancesAsync` sums them with the sign of R7. At most 100 ids (the request line of 200 ids is near the server's 8 KB limit; a list page holds 20); more is `400 finance.too_many_ids`.

## 4. Tasks (at most 8)
- [x] 1. Query use case and the cap (R17).
- [x] 2. Routes `ledger/balances/schools` and `ledger/balances/vendors` for the Owner and an entered Super Admin.
- [x] 3. Build at 0 warnings, 0 errors; verify section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `AC9` | 1, 2 | Owner of A, `ledger/balances/schools?ids=S&ids=S2&ids=<B's school>` -> 200 `{S2: 4650.00, S: 20.00}` = their statements' closing balances (4650.00, 20.00), B's left out; vendor V -> 2500.00; 101 ids -> 400 `finance.too_many_ids`; Staff 403; anonymous 401. **PASS** |

## 6. Not verified
Nothing. Run by `check_fin05.py` (scratch, not committed) on the real `book_inventory`: 4/4 passed. First run: 201 ids answered 414 (the request line passed the server's 8 KB limit), so the cap became 100 ids (spec R17 corrected the same day).

## 7. Risks and notes
None.
