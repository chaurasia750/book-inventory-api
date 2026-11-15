# Plan item#05: Excel import and bulk update of items

- **File:** `docs/plans/item/2026-11-15-05-excel-import.md`
- **Kind:** Backend
- **Spec:** [../../specs/item.md](../../specs/item.md) - rules covered: R18-R22; AC14-AC16
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** docs/item-import
- **Depends on:** `item#01` (Done), `stock#03` (Done, the shared spreadsheet reader and writer)
- **UI plan:** item#06
- **Approved by / on:** abhishek / 2026-11-15 (chat: Bulk Upload option asked; Q13-Q15 answered with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the Owner uploads an Excel file of books: new books are created (with their generated codes and barcodes) and existing ones are updated, all or nothing, with a report of every wrong row. Try it: `GET /api/v1/items/import/template`, fill it, `POST /api/v1/items/import`.

## 2. Touches / leaves out
Touches: `Application/Item` (the import use case, the row parsing, the result and report records), the existing `ISpreadsheetReader` and `ISpreadsheetWriter` (no new package), the item aggregate's create, edit and barcode behaviours (reused, not copied), `Api/Controllers/Item` (the two endpoints), the message catalogue.
Leaves out: schools, vendors and prices imports; the screen (item#06). No schema change.

## 3. Approach
Mirror the opening stock import (stock R17-R19): read the first sheet, check the file, parse and check every row (format, then against the company's items by code or barcode and vendors by name, and against the rest of the file), report all wrong rows, and only when none is wrong create and update through the item's own behaviours in the request transaction. The class is normalised to the standard spelling when it matches; `-` clears an optional value; a blank keeps it.

## 4. Tasks (at most 8)
- [x] 1. Row parsing and value rules for the ten columns, reusing the item's rules (R19, R20).
- [x] 2. Lookups: items by code or barcode, vendors by name, barcodes in use (R19, R20) through the capabilities.
- [x] 3. The use case: file checks, whole-file validation with the full report, then create and update (R21, R22).
- [x] 4. The template and the two endpoints with roles; catalogue; build at 0 warnings.
- [x] 5. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R18`, `AC15` | 3, 4 | The template downloads with the ten headers; a `.csv`, a file above 5 MB and 5001 rows -> 400. **Observed:** template: 200, headers = the ten columns; `items.csv` -> 400 `item.import.invalid_file`; 5001 rows -> 400 `item.import.too_many_rows`; a file of 5 MB + 10 bytes -> 400 `item.import.too_large` |
| `R19`-`R21`, `AC14` | 1-3 | The file of 3 new, 2 updates by code (one blank class, one `-` subject) and 1 by barcode -> created 3, updated 3; the consecutive codes, the kept class, the cleared subject, the added barcode checked in the database. **Observed:** prelim file of 3 rows -> `created 3`; AC14 file (3 new, 2 by code, 1 by barcode) -> `{created: 3, updated: 3}`; new codes ITM-0719/0720/0721 in file order, first named `New Book One` with barcode `NEWBOOK<run>` on it, class `2ND` stored as `2nd`, HSN 4901, GST 12; blank class stayed `LKG` while the name changed; `-` cleared the subject, class `1st` stayed; update by barcode set GST 12 and stock yes, old barcode kept; a barcode added by code -> 2 barcodes; vendor (typed in capitals) matched, `-` cleared it, a name set it |
| `R22`, `AC15` | 3 | The file with the unknown code, the foreign barcode, category `XYZ`, GST 7, unknown vendor, the same item twice and a nameless new item -> 400 with exactly those rows, the item count unchanged. **Observed:** 9-row file -> 400 `item.import.rows_invalid` with exactly: row 3 ItemCode `item_unknown`, 4 Barcode `barcode_taken`, 5 Category `invalid_category`, 6 GstRate `invalid_gst_rate`, 7 Vendor `vendor_unknown`, 9 ItemCode `duplicate_row`, 10 Name `name_required`; the good row 2 and the first of the twice rows changed nothing: item count 721 before and after, the three items unchanged; an inactive vendor on a new item -> `vendor_unknown`, an item that already has it may stay |
| `R18`, `AC16` | 4 | Staff -> 403; the Owner of B with A's `ITM-` code -> that row unknown. **Observed:** Staff -> 403 on the import and on the template; the Owner of ABC Book Center with an ITM- code of Children Book Center -> 400, row 2 `item_unknown`, the item unchanged |

## 6. Not verified
Not seen in a browser (the screen is item#06). The Excel file was built with openpyxl, not opened in Excel itself. Row order of a file with the same new item twice was not exercised beyond the barcode and code cases above.

## 7. Risks and notes
No migration. A new item needs a category as well as a name (a blank category on a new row is a wrong row, `item.import.invalid_category`): R20 only says the category is one of the seven; recorded in the spec. The class standard names come from the school module default list (`School.DefaultClasses`). Test items ITM-0716 to ITM-0722 and one test vendor were created in the local `book_inventory` and deactivated afterwards. The check file for 5001 rows is generated by the scratch script. Vendors are matched by name only (a duplicate vendor name matches the first; vendor names are unique by their own rule).
