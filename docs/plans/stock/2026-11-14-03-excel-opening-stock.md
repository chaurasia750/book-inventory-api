# Plan stock#03: Excel opening stock

- **File:** `docs/plans/stock/2026-11-14-03-excel-opening-stock.md`
- **Kind:** Backend
- **Spec:** [../../specs/stock.md](../../specs/stock.md) - rules covered: R17-R19; AC9, AC10
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `stock#01`
- **UI plan:** stock#04
- **Approved by / on:** abhishek / 2026-11-14 (chat: Q9 "now", ClosedXML, three columns with one date per file, all or nothing, 5000 rows / 5 MB / `.xlsx`)
- **Created:** 2026-11-14

## 1. In one minute
After this plan the Owner can download an Excel template, fill in item code or barcode, stock type and quantity, and upload it with one opening date; either every row becomes an opening entry, or nothing is imported and a report lists every wrong row with its reason. Try it: `GET /api/v1/stock/opening/template`, fill it, `POST /api/v1/stock/opening/import` with the file and `openingDate`.

## 2. Touches / leaves out
Touches: `Directory.Packages.props` and `BookInventory.Infrastructure.csproj` (the package ClosedXML, approved by the owner, D5), `BookInventory.Application/Stock` (the import use case, the row checks and the report contracts, a port to read rows), `BookInventory.Infrastructure/Stock` (the ClosedXML reader and the template writer), `BookInventory.Api/Controllers/Stock` (two endpoints, the multipart upload with its size limit), the message catalogue, `docs/Code_Map.md`.
Leaves out: the upload screen (stock#04), imports of other masters (they reuse the reader later), keeping the file.
Schema change: no.

## 3. Approach
A small port `ISpreadsheetReader` (Application) returns the rows of the first sheet as text cells; its ClosedXML adapter lives in Infrastructure, so the use case never sees the package and later imports reuse it. The use case checks every row before writing anything: the file type and size (5 MB, `.xlsx`), the row count (5000), each row's code or barcode through the item directory (item R16, R17: found, stock managed), the stock type, the quantity (whole, greater than zero), no existing opening (R7) and no duplicate item and stock type in the file. Any wrong row -> `400` with the report (row number, column, message code) and nothing written; all right -> the openings are posted through the same `ItemStock` behaviour as a typed opening (R19), in the request transaction. The item lookup by code or barcode is a new read of the item directory; if it needs a new method the plan adds it to item R16's capability in the same small way as item#04.

## 4. Tasks (at most 8)
- [ ] 1. Add ClosedXML to `Directory.Packages.props` and Infrastructure (own commit, the owner's approval named in the message).
- [ ] 2. Port `ISpreadsheetReader` and its ClosedXML adapter; the template writer (R17).
- [ ] 3. The item lookup by code or barcode through the item directory (R17, R18).
- [ ] 4. Use case `ImportOpeningStock`: file limits, row checks, report, all or nothing, posting through `ItemStock` (R17, R18, R19).
- [ ] 5. Endpoints `GET stock/opening/template` and `POST stock/opening/import` (Owner and entered Super Admin; request size limit).
- [ ] 6. Message catalogue; build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 7. Verify every row of section 5 on the real local database with generated test files (scratch, not committed) and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R17`, `R19`, `AC9` | 2, 4, 5 | The template downloads with the three columns; a file with a book by code, a copy by barcode and a book as `DAMAGED`, date 2026-04-01 -> 3 imported; the stock list and history show `OPENING` entries dated 2026-04-01 with the file's name in the note; the same file again -> 400 report "opening exists" for the three rows, nothing imported |
| `R18`, `AC10` | 4 | One right row plus an unknown code, a pen, quantity 0, quantity 2.5 and the same item twice -> 400, the report names exactly those rows with their codes, and SQL shows no entry written |
| `R17`, `AC10` | 5 | A `.csv` -> 400; a file above 5 MB -> 400 (or 413 by the server limit, recorded as observed); 5001 rows -> 400 |
| `R10` | 5 | Staff on both endpoints -> 403; anonymous -> 401 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
ClosedXML is the first spreadsheet package in the solution; its version is pinned in `Directory.Packages.props`. A 5000-row import checks 5000 items through the directory in batches, not one by one.
