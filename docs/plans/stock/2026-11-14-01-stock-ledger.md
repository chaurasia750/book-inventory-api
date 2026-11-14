# Plan stock#01: Stock ledger

- **File:** `docs/plans/stock/2026-11-14-01-stock-ledger.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R1-R16; AC1-AC8
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `platform#01` (Done), `item#04` (Approved)
- **UI plan:** stock#02
- **Approved by / on:** abhishek / 2026-11-14 (chat: the open questions answered one by one)
- **Created:** 2026-11-14
- **Base branch:** `docs/stock-ledger` (one branch `feature/stock` for item#04, stock#01 and stock#03, the owner's instruction)

## 1. In one minute
After this plan the Owner can enter the opening stock of a book and correct it after a count, everyone can see the current Main, Damaged and Sample stock of each book and its history, and later modules have one way to post stock movements that never blocks a sale. Try it: as an Owner `POST /api/v1/items/{itemId}/stock/opening` with `{stockType: "MAIN", quantity: 100, openingDate: "2026-04-01"}`, then `GET /api/v1/stock` shows 100.

## 2. Touches / leaves out
Touches: new `BookInventory.Domain/Stock`, `BookInventory.Application/Stock` (use cases, query service and the posting capability `IStockPosting`), `BookInventory.Infrastructure/Stock` and `Persistence` (schema `stock`, migration), `BookInventory.Api/Controllers/Stock`, `CLAUDE.project.md` (the module list gets `stock`, in its own commit), `docs/Code_Map.md`.
Leaves out: the document modules that will call the posting capability, Excel opening stock (stock#03), the stock value report, any UI (stock#02).
Schema change: yes (new schema `stock`, additive).

## 3. Approach
`ItemStock` derives from `CompanyOwnedRoot`, one per item, created on its first movement; it keeps the three running totals and appends `StockEntry` children (`TransactionDomainBase`-style insert-only rows, company-owned). Every movement goes through one root method, `Post(movement)`, which validates the quantity, appends the entry and updates the total, so a total and its entries can never disagree; the concurrency token of the root serialises parallel postings of one item (a conflict surfaces as `409` and the caller may retry). Loading the root does not load its history: new entries are appended without reading the old ones. Opening (once per item and type) and adjustment (counted quantity, difference posted, reason) are root behaviours. `IStockPosting` (Application) takes a document's lines, asks the item directory (item R17) which items are stock managed, skips the others, refuses a document already posted, and reverses exactly what a document posted on cancel. The business date comes from the existing `BusinessCalendar`. The history read computes the quantity after each entry from the entries themselves.

## 4. Tasks (at most 8)
- [x] 1. Domain: `ItemStock` (post, opening, adjustment, totals), `StockEntry`, stock types and movement kinds with their direction, events, message codes (R2-R8, R12, R15).
- [x] 2. Persistence: configurations (unique item per company, unique document posting, indexes), schema `stock`, migration; add `stock` to the module list of `CLAUDE.project.md` (own commit); show the SQL and the target database `book_inventory` to the user before applying.
- [x] 3. Use cases `EnterOpening` and `AdjustStock` (Owner only) (R7, R8, R10).
- [x] 4. Posting capability `IStockPosting` (post a document, reverse a document; skip non-stock items; report items at or below zero) (R2, R9, R11).
- [x] 5. Query service: the stock list through the item directory with search, filters and sort; one item's stock with its history and the quantity after each entry (R10).
- [x] 6. `StockController` with the roles of the API table (R10).
- [x] 7. Message catalogue and registration; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 8. Verify every row of section 5 on the real local database (the posting capability through a scratch caller that is not committed) and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`, `R4`, `R7`, `R15`, `AC1` | 1, 3 | Opening 100 MAIN for book X dated 2026-04-01 -> 201 `quantity: 100`; second MAIN opening -> 409 `stock.opening_exists`; 5 DAMAGED -> 201 `quantity: 5`; pen -> 400 `stock.not_stock_managed`; quantity 0 -> 400, -3 -> 400, 2.5 -> 400. **PASS** |
| `R8`, `AC2` | 1, 3 | Counted 97, reason "physical count" -> 201 `ADJUSTMENT`, in 0, out 3, `quantity: 97`; 97 again -> 400 `stock.nothing_to_adjust`; reason "abc" -> 400 (validation, Reason length 5-500); Staff of B -> 403. **PASS** |
| `R4`, `R6`, `AC3` | 5 | `GET items/X/stock` -> entries newest first: REVERSAL MAIN 97, SCHOOL_INVOICE MAIN -23, ADJUSTMENT MAIN 97, OPENING DAMAGED 5, OPENING MAIN 100 (quantity after each); SQL sum(in - out) per type `DAMAGED=5,MAIN=97` = response 97 / 5, Sample 0. **PASS** |
| `R2`, `R6`, `R9`, `R11`, `AC4` | 4 | Scratch controller (deleted) calling `IStockPosting`: document SCRATCH INV-T1 dated 2026-05-10 with X MAIN SCHOOL_INVOICE out 120 and a pen out 2 -> `entriesPosted: 1`, `itemsAtOrBelowZero: [X]`, SQL main 97 -> -23, 1 entry for the document, no stock card for the pen; same document again -> 409 `stock.document_already_posted`; reverse -> one `REVERSAL:120:0`, main back to 97; reverse again -> 409 `stock.document_already_reversed`. **PASS** |
| `R1`, `AC5` | 5, 6 | Owner of B: `GET stock?search=<X>` -> 0 rows; A's X -> 404 `stock.item_not_found`, unknown id -> 404 same code; Owner of A with `?companyId=<B>` -> still A's 1 row; opening with `companyId: <B>` in the body -> 201 and SQL company of the card = ABC Book Center. **PASS** |
| `R9`, `R10`, `AC6` | 5 | `pageSize=500` -> 200; search lists only the two books (pen not listed); X row 97/5/0 no mark, Y row 0/0/4 marked; `atOrBelowZeroOnly=true` -> only Y; `orderBy=ItemName DESC` -> Y, X; category PEN -> 0, BOOK -> 2; zero-or-below total of A 16 = SQL 16. **PASS** |
| `R10`, `AC7` | 6 | Super Admin, no company: `GET stock` -> 200 empty, opening -> 403 `core.company_required`; after entering A: `GET items/X/stock` -> 200, SAMPLE opening -> 201. **PASS** |
| `R4`, `R13`, `AC8` | 6 | `PUT` and `DELETE items/X/stock` -> 405, 405; every history entry has `createdBy` and `createdAt`, the Owner's name "ABC Owner" shown. **PASS** |
| `R12`, `R16` | 3, 4 | Adjustment `movement_date` 2026-11-14 = SQL today in Asia/Kolkata; the document entry carries 2026-05-10; Y deactivated (204): hidden from the list, listed with `includeInactive=true`, SAMPLE adjustment to 3 -> 201. Staff of B: `GET stock` 200, item stock 200, opening 403; anonymous 401. **PASS** |

## 6. Not verified
Nothing of section 5. Run by `check_stock01.py` (scratch, not committed): 36/36 passed on the real `book_inventory`.

Migration `Stock_Initial` (new schema `stock`, tables `item_stock` and `stock_entry`, two check constraints, five indexes; additive): the SQL was generated and shown in the session, target the local `book_inventory`; it was applied there by the app's start (owner's standing decision for the local database). Rollback: drop schema `stock`.

Notes: the warning mark and the "zero or below" filter look at the **Main** quantity (the sellable stock, R9); the "zero or below" filter reads the matching items in pages of 200 (at most 5000) because the quantities live in this module; the company guard and the business calendar are reused from `Application/Price` (one rule, one place); the history is not paged.

## 7. Risks and notes
The migration is additive; rollback: drop schema `stock`. A very busy item (many parallel postings) may see `409` retries on its running total; the document modules retry once. If the owner answers Q3 with "+/- quantities", the adjustment input changes before the plan starts.
