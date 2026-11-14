# Plan stock#01: Stock ledger

- **File:** `docs/plans/stock/2026-11-14-01-stock-ledger.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R1-R16; AC1-AC8
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `platform#01` (Done), `item#04` (Approved)
- **UI plan:** stock#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner can enter the opening stock of a book and correct it after a count, everyone can see the current Main, Damaged and Sample stock of each book and its history, and later modules have one way to post stock movements that never blocks a sale. Try it: as an Owner `POST /api/v1/items/{itemId}/stock/opening` with `{stockType: "MAIN", quantity: 100, openingDate: "2026-04-01"}`, then `GET /api/v1/stock` shows 100.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Stock`, `BookInventory.Application/Stock` (use cases, query service and the posting capability `IStockPosting`), `BookInventory.Infrastructure/Stock` and `Persistence` (schema `stock`, migration), `BookInventory.Api/Controllers/Stock`, `CLAUDE.project.md` (the module list gets `stock`, in its own commit), `docs/Code_Map.md`.
Leaves out: the document modules that will call the posting capability, Excel opening stock (stock#03), the stock value report, any UI (stock#02).
Schema change: yes (new schema `stock`, additive).

## 3. Approach
`ItemStock` derives from `CompanyOwnedRoot`, one per item, created on its first movement; it keeps the three running totals and appends `StockEntry` children (`TransactionDomainBase`-style insert-only rows, company-owned). Every movement goes through one root method, `Post(movement)`, which validates the quantity, appends the entry and updates the total, so a total and its entries can never disagree; the concurrency token of the root serialises parallel postings of one item (a conflict surfaces as `409` and the caller may retry). Loading the root does not load its history: new entries are appended without reading the old ones. Opening (once per item and type) and adjustment (counted quantity, difference posted, reason) are root behaviours. `IStockPosting` (Application) takes a document's lines, asks the item directory (item R17) which items are stock managed, skips the others, refuses a document already posted, and reverses exactly what a document posted on cancel. The business date comes from the existing `BusinessCalendar`. The history read computes the quantity after each entry from the entries themselves.

## 4. Tasks (at most 8)
- [ ] 1. Domain: `ItemStock` (post, opening, adjustment, totals), `StockEntry`, stock types and movement kinds with their direction, events, message codes (R2-R8, R12, R15).
- [ ] 2. Persistence: configurations (unique item per company, unique document posting, indexes), schema `stock`, migration; add `stock` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [ ] 3. Use cases `EnterOpening` and `AdjustStock` (Owner only) (R7, R8, R10).
- [ ] 4. Posting capability `IStockPosting` (post a document, reverse a document; skip non-stock items; report items at or below zero) (R2, R9, R11).
- [ ] 5. Query service: the stock list through the item directory with search, filters and sort; one item's stock with its history and the quantity after each entry (R10).
- [ ] 6. `StockController` with the roles of the API table (R10).
- [ ] 7. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 8. Verify every row of section 5 on the real local database (the posting capability through a scratch caller that is not committed) and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R4`, `R7`, `R15`, `AC1` | 1, 3 | Opening 100 Main for book X -> 201, quantity 100; second opening -> 409; opening 5 Damaged -> 201; opening for a pen -> 400; quantities 0, -3, 2.5 -> 400 |
| `R8`, `AC2` | 1, 3 | Adjustment counted 97 with a reason -> one `ADJUSTMENT` out 3, quantity 97; counted 97 again -> 400; reason of 3 characters -> 400; Staff -> 403 |
| `R4`, `R6`, `AC3` | 5 | The history lists entries newest first with the quantity after each; in minus out per type equals the quantity (SQL) |
| `R2`, `R6`, `R9`, `R11`, `AC4` | 4 | Scratch caller posts a document: X Main out 120 and a pen out 2 -> one entry for X, Main 97 -> -23 without refusal, X reported at or below zero; the same document again -> refused; reverse -> one `REVERSAL` in 120, Main back to 97 |
| `R1`, `AC5` | 5, 6 | Owner of B: list shows none of A's; A's item stock -> 404 identical to an unknown item; `companyId` in query and body ignored |
| `R9`, `R10`, `AC6` | 5 | List paged and capped at 200; search, category, vendor, class, "zero or below" filters and sorts; each row shows the three quantities and the warning mark |
| `R10`, `AC7` | 6 | Super Admin with nothing entered -> empty list, write -> 403 `core.company_required`; after entering A -> reads and enters an opening |
| `R4`, `R13`, `AC8` | 6 | No route changes or deletes an entry (`PUT`/`DELETE` -> 405); each entry carries who and when |
| `R12`, `R16` | 3, 4 | An adjustment is dated today (business date); a document entry carries the document's date; a deactivated item can still be adjusted |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
The migration is additive; rollback: drop schema `stock`. A very busy item (many parallel postings) may see `409` retries on its running total; the document modules retry once. If the owner answers Q3 with "+/- quantities", the adjustment input changes before the plan starts.
