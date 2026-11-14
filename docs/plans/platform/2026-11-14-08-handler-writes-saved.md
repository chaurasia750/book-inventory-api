# Plan platform#08: Writes of domain event handlers are saved in the same transaction

- **File:** `docs/plans/platform/2026-11-14-08-handler-writes-saved.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P24; PAC19
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** none
- **UI plan:** none, it changes how a save works inside the shared library; nothing visible
- **Approved by / on:** abhishek / 2026-11-14 (chat: "Platform fix: handler ka kaam bhi usi transaction mein save")
- **Created:** 2026-11-14
- **Base branch:** `docs/finance-ledger` (one branch `feature/finance` for platform#08, school#03, school#04, vendor#04 and finance#01-#03, the owner's instruction "all plan done kro")

## 1. In one minute
Today a domain event handler runs after the save, and whatever it adds is never saved. After this plan the save repeats while handlers leave changes (at most five rounds), inside the same transaction, so the ledger's opening-balance handler (finance#02) is saved with the school or vendor change. Seen working through finance#02's checks and one throw-away failing handler.

## 2. Touches / leaves out
Touches: `library/BuildingBlocks/Persistence/BuildingBlocksDbContext.cs` (`SaveChangesAsync`), `docs/Code_Map.md` (a line on handlers).
Leaves out: the dispatcher itself, any handler (finance#02 adds the first), outbox or background events.
Schema change: no.

## 3. Approach
`SaveChangesAsync` keeps its order (stamp, collect events, save, dispatch) and then loops: if the change tracker has added, modified or deleted entries or roots with new events, it stamps, collects and saves again; after five rounds it throws `InvalidOperationException` (a bug, `500`). The request transaction of `TransactionFilter` already wraps all rounds, so a failing handler rolls everything back. The returned count is the sum of the rounds. Generic, no app names, so it stays in `library/`.

## 4. Tasks (at most 8)
- [x] 1. The save loop in `BuildingBlocksDbContext.SaveChangesAsync` with the five-round cap (P24).
- [x] 2. Build at 0 warnings, 0 errors; `docs/Code_Map.md`.
- [x] 3. Verify every row of section 5 (with finance#02 and a throw-away handler) and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P24`, `PAC19` | 1 | School created with 5000.00: the school and its OPENING_BALANCE transaction saved in the same request (finance#02 handler, second save round, company stamped A); a throw-away `VendorCreated` handler that throws -> 500 and SQL: no vendor `P24 FAIL ...` and no ledger rows; regression: item create 201, stock#03 checks re-run after the change: 14/14 passed (imports, openings, row reports). **PASS** |

## 6. Not verified
Run by `check_finance.py` (scratch, not committed; a temporary scratch controller and a throw-away failing handler were deleted before the commit): 28/28 passed on the real `book_inventory`.

Notes: `AppDbContext` now stamps the company and captures the audit trail in `OnSavingRound()` (a new protected hook of `BuildingBlocksDbContext`), so the rows of every round are stamped and audited; the sync `SaveChanges` is unchanged (it still refuses pending events).

## 7. Risks and notes
Shared library change, kept in its own commit. A handler that changes something on every round is caught by the cap instead of looping forever.
