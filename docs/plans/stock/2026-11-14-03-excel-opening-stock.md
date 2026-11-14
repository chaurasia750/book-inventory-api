# Plan stock#03: Excel opening stock

- **File:** `docs/plans/stock/2026-11-14-03-excel-opening-stock.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R17-R19; AC9, AC10
- **Owner:** abhishek
- **Status:** Done (2026-11-14)
- **Depends on:** `stock#01`
- **UI plan:** stock#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q9 "now", ClosedXML, three columns with one date per file, all or nothing, 5000 rows / 5 MB / `.xlsx`)
- **Created:** 2026-11-14
- **Base branch:** `docs/stock-ledger` (one branch `feature/stock` for item#04, stock#01 and stock#03, the owner's instruction)

## 1. In one minute
After this plan the Owner can download an Excel template, fill in item code or barcode, stock type and quantity, and upload it with one opening date; either every row becomes an opening entry, or nothing is imported and a report lists every wrong row with its reason. Try it: `GET /api/v1/stock/opening/template`, fill it, `POST /api/v1/stock/opening/import` with the file and `openingDate`.

## 2. Touches / leaves out
Touches: `Directory.Packages.props` and `BookInventory.Infrastructure.csproj` (the package ClosedXML, approved by the owner, D5), `BookInventory.Application/Stock` (the import use case, the row checks and the report contracts, a port to read rows), `BookInventory.Infrastructure/Stock` (the ClosedXML reader and the template writer), `BookInventory.Api/Controllers/Stock` (two endpoints, the multipart upload with its size limit), the message catalogue, `docs/Code_Map.md`.
Leaves out: the upload screen (stock#04), imports of other masters (they reuse the reader later), keeping the file.
Schema change: no.

## 3. Approach
A small port `ISpreadsheetReader` (Application) returns the rows of the first sheet as text cells; its ClosedXML adapter lives in Infrastructure, so the use case never sees the package and later imports reuse it. The use case checks every row before writing anything: the file type and size (5 MB, `.xlsx`), the row count (5000), each row's code or barcode through the item directory (item R16, R17: found, stock managed), the stock type, the quantity (whole, greater than zero), no existing opening (R7) and no duplicate item and stock type in the file. Any wrong row -> `400` with the report (row number, column, message code) and nothing written; all right -> the openings are posted through the same `ItemStock` behaviour as a typed opening (R19), in the request transaction. The item lookup by code or barcode is a new read of the item directory; if it needs a new method the plan adds it to item R16's capability in the same small way as item#04.

## 4. Tasks (at most 8)
- [x] 1. Add ClosedXML to `Directory.Packages.props` and Infrastructure (own commit, the owner's approval named in the message).
- [x] 2. Port `ISpreadsheetReader` and its ClosedXML adapter; the template writer (R17).
- [x] 3. The item lookup by code or barcode through the item directory (R17, R18).
- [x] 4. Use case `ImportOpeningStock`: file limits, row checks, report, all or nothing, posting through `ItemStock` (R17, R18, R19).
- [x] 5. Endpoints `GET stock/opening/template` and `POST stock/opening/import` (Owner and entered Super Admin; request size limit).
- [x] 6. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 7. Verify every row of section 5 on the real local database with generated test files (scratch, not committed) and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `R19`, `AC9` | 2, 4, 5 | `GET stock/opening/template` -> 200 `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet` with the headers "Item code or barcode", "Stock type", "Quantity"; a file with book `ITM-0045` by code MAIN 10, a copy by its barcode with empty type 20, the same book in lower case `damaged` 3, date 2026-04-01 -> 200 `imported: 3`; SQL: three `OPENING` entries dated 2026-04-01, note `opening-april.xlsx`; the stock list shows 10 / 3; the same file again -> 400 `stock.import.rows_invalid`, rows 2, 3, 4 each `stock.opening_exists`, still 3 entries. **PASS** |
| `R18`, `AC10` | 4 | One right row plus: unknown code (row 3 `stock.import.item_unknown`), a pen (row 4 `stock.not_stock_managed`), quantity 0 (row 5 `stock.invalid_quantity`), 2.5 (row 6 `stock.invalid_quantity`), the same book DAMAGED twice (row 8 `stock.import.duplicate_row`), type `BROKEN` (row 9 `stock.import.invalid_stock_type`) -> 400 with exactly those six rows; SQL entry count 14 before = 14 after, none for the right row's book. **PASS** |
| `R17`, `AC10` | 5 | `.csv` -> 400 `stock.import.invalid_file`; a CSV renamed `.xlsx` -> 400 `stock.import.invalid_file`; 5 MB + 10 bytes -> 400 `stock.import.too_large`; 7 MB -> 400 from the server body limit (6 MB): "Request body too large. The max request body size is 6291456 bytes." (framework validation problem, no module code); 5001 rows -> 400 `stock.import.too_many_rows`; exactly 5000 rows are read (5000 rows reported unknown); no opening date -> 400. **PASS** |
| `R10` | 5 | Staff: template 403, import 403; anonymous: 401, 401; the Owner of B uploading A's code -> row `stock.import.item_unknown` (A's items invisible, P19). **PASS** |

## 6. Not verified
Nothing of section 5. Run by `check_stock03.py` (scratch, not committed; the test files were generated as minimal `.xlsx` zips by the script): 14/14 passed on the real `book_inventory`.

Notes: ClosedXML 0.105.1 is pinned in `Directory.Packages.props` (own commit). The port is split in `ISpreadsheetReader` and `ISpreadsheetWriter` (`Application/Core`, for the later imports) with one ClosedXML adapter each in `Infrastructure/Core`. The item lookup by code or barcode was added to the item directory (`FindByCodeOrBarcodeAsync`, a code wins over another item's barcode), as section 3 foresaw. A report row names its column: the item column for an unknown or not stock-managed item, the stock-type column for an existing opening or a repeat in the file.

## 7. Risks and notes
ClosedXML is the first spreadsheet package in the solution; its version is pinned in `Directory.Packages.props`. A 5000-row import checks 5000 items through the directory in batches, not one by one.
