# Spec: stock - Stock

- **Module:** `stock` (PostgreSQL schema `stock`)
- **Status:** R1-R19 Approved by abhishek in chat on 2026-11-14 (Q1-Q10 answered one by one; Q9 answered "now", with the Excel package ClosedXML and the import rules R17-R19 answered the same way; D1-D5 with them)
- **Last updated:** 2026-11-14
- **Source docs:** [BRD §1.3, §2, §3, §4.6 Stock (FR-STK-01 to FR-STK-10), §4.12 Return (FR-RET-04, FR-RET-11), §4.13 Sample (FR-SMP-01), §5.5 BR-STK and its stock movement table, §8 Data Model (StockLedger) and design decision 2, §10, §11 item 10](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P23; [specs/company.md](company.md); [specs/item.md](item.md)
- **Plans:** [stock#01](../plans/stock/2026-11-14-01-stock-ledger.md) (Done), [stock#02](../plans/stock/2026-11-14-02-ui-stock-screens.md) (Approved), [stock#03](../plans/stock/2026-11-14-03-excel-opening-stock.md) (Approved), [stock#04](../plans/stock/2026-11-14-04-ui-excel-opening-stock.md) (Approved); the capability it needs from item: [item#04](../plans/item/2026-11-14-04-item-directory-stock-flag.md) (Done)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`stock` counts how many pieces of each stock-managed item (books and copies) the company has. Every movement is one entry in a stock ledger; the quantity is the sum of the entries, never a number typed over (BRD §8 design decision 2). Three stocks are kept apart: **Main**, **Damaged** and **Sample** (BR-STK-02).

Feature **Stock ledger** (R1-R16): the Owner enters the opening stock and corrects the stock after a physical count (adjustment with a reason); everyone with access sees the current stock and the history of an item; later document modules (Purchase, School Invoice, Counter Sale, Return, Sample) post their movements through this module's posting capability, which never refuses a sale for low stock (BR-STK-03).

Feature **Excel opening stock** (R17-R19, FR-STK-10, NFR-12): the Owner uploads an Excel file of opening quantities instead of typing them one by one.

Out of scope for these features: the documents that cause most movements (Purchase, Invoice, Counter Sale, Return, Sample: their own modules, which call the posting capability of R11); the stock value and profit report (§4.16, the Reports module); batch-wise stock and transfers between godown and counter (BRD §10, later phases); the Super Admin's stock screens in the `admin` app (Q10).

## 2. Concepts and glossary

- **Stock-managed item**: an item whose quantity is counted; by default Book and Copy ([specs/item.md](item.md) R4, BR-STK-01).
- **Stock type**: `MAIN` (sellable), `DAMAGED` (kharab: returned in bad condition, can go back to the vendor) or `SAMPLE` (given free by vendors, lent to schools) (BR-STK-02, FR-STK-07, FR-STK-08).
- **Stock entry**: one movement: item, stock type, quantity in or out, date, movement kind, the document it comes from and who made it (FR-STK-03).
- **Quantity**: per item and stock type, the sum of the quantities in minus the quantities out (FR-STK-02).
- **Business date**: "today" in the time zone `Asia/Kolkata` ([specs/company.md](company.md) D7).

## 3. Business rules

**Stock ledger**
- **R1** Stock belongs to exactly one company, the company of the session. Another company's stock is never read or changed (platform P18-P21). Items are read through the item module's directory capability ([specs/item.md](item.md) R16, R17), never by joining its tables.
- **R2** Only **stock-managed** items have stock (FR-STK-01, BR-STK-01). Opening stock or an adjustment for an item that is not stock managed is refused (`400`). A document movement for such an item is skipped without an entry (R11), because documents mix books with pens and bags.
- **R3** An item has **one quantity per stock type**: Main, Damaged, Sample (FR-STK-02, BR-STK-02). Old and new stock are not kept apart (no batches, BRD §10).
- **R4** Every movement is **one stock entry** (FR-STK-03): item, stock type, movement kind (R5), quantity in or quantity out (a whole number greater than zero, Q1), the movement date, the document type, document id and document number it comes from (empty for opening and adjustment), a note, and who made it and when. Entries are **insert-only**: they are never edited or deleted; a mistake is corrected by a new entry (an adjustment, or a reversing entry from the document's cancel).
- **R5** The **movement kinds** and their stock and direction come from the BRD movement table (BR-STK, FR-STK-04, FR-STK-05): `OPENING` (in, any stock type), `ADJUSTMENT` (in or out, any stock type), `PURCHASE` (Main in), `SCHOOL_INVOICE` (Main out), `COUNTER_SALE` (Main out), `SCHOOL_RETURN_GOOD` (Main in), `SCHOOL_RETURN_DAMAGED` (Damaged in), `VENDOR_RETURN` (Main or Damaged out), `SAMPLE_FROM_VENDOR` (Sample in), `SAMPLE_TO_SCHOOL` (Sample out), `SAMPLE_FROM_SCHOOL` (Sample in), `SAMPLE_TO_VENDOR` (Sample out), and `REVERSAL` (the opposite of an earlier entry, when a document is cancelled). A vendor order never moves stock (BR-STK-04); an invoice made from a sample does not reduce Main stock (BR-STK-05). This module stores the kinds; the document modules decide when to post them.
- **R6** The **quantity** of an item and stock type is the sum of its entries' quantities in minus quantities out (BRD §8 design decision 2). It may be kept as a running total for speed, but it always equals that sum. It **may go below zero** (BR-STK-03, FR-STK-06).
- **R7** **Opening stock** (FR-STK-04): the Owner enters, for one item and one stock type, the quantity and the date the system starts counting from. An item and stock type has at most one opening entry (Q2); a later correction is an adjustment (R8).
- **R8** **Adjustment** (FR-STK-09): only the Owner corrects the stock after a physical count. The Owner gives the item, the stock type, the **counted quantity** and a **reason** (5-500 characters) (Q3); the system posts the difference as one adjustment entry (in when the count is higher, out when lower). A count equal to the current quantity posts nothing and is refused (`400`), so no empty entry exists.
- **R9** **Low stock never blocks** (FR-STK-06, BR-STK-03): no posting is ever refused because the quantity is low, zero or would go negative. A quantity of zero or less is shown as a warning in the stock list and the item's stock, and the posting capability tells the caller which items went to zero or below so that documents can show their yellow warning (FR-ORD-08) (Q4).
- **R10** **Reading stock**: the Owner, Staff and an entered Super Admin read the **current stock list** (one row per stock-managed item: Main, Damaged and Sample quantities, a warning mark at zero or below), paged (page size capped at 200), searchable by item name, code, barcode, class, subject and vendor name and filterable by category, vendor, class and "zero or below only" through the item directory, sortable by item name and code; and the **history** of one item: its entries newest first with the quantity after each entry per stock type (Q5). Writing (opening, adjustment) is the Owner's and an entered Super Admin's; Staff's writes answer `403`, anonymous `401`. With no company entered a Super Admin reads nothing and a write is refused (`403 core.company_required`, platform P20).
- **R11** **Posting capability for other modules**: the document modules post their movements through one capability of this module, inside their own request transaction: a list of lines (item, stock type, movement kind, quantity in or out) with the document type, id, number and date. Lines for items that are not stock managed are skipped (R2). The same document line is never counted twice: posting a document type and id that was already posted is refused, and a cancel posts `REVERSAL` entries for exactly what that document posted (Q6). No other module writes stock entries.
- **R12** The movement **date** is the date of the document (or the opening date the Owner gives), not the time it was saved; an adjustment is dated today (business date). The time it was saved is kept separately (who and when).
- **R13** The history of stock is its entries (R4): each entry names who made it and when, so this module needs no audit trail of its own for entries; the running totals, if kept, are not audited (they are derived).
- **R14** A stock entry never changes a price or a ledger: money movements belong to the Payment and Ledger modules (BR-STK movement table, last two columns).
- **R15** Stock quantities are whole numbers stored as integers; no rounding happens in this module (Q1).
- **R16** An item that is deactivated keeps its stock and history; its stock can still be adjusted, and the stock list shows it under a filter (Q7).

**Excel opening stock**
- **R17** The Owner (and an entered Super Admin) uploads an `.xlsx` file of opening stock (FR-STK-10, NFR-12): one row per item and stock type with the columns **item code or barcode**, **stock type** (`MAIN`, `DAMAGED` or `SAMPLE`; empty means `MAIN`) and **quantity** (a whole number greater than zero); the **opening date** is given once on the screen for the whole file. A template file with these columns can be downloaded. The file is at most 5 MB and 5000 rows; anything else is refused (`400`).
- **R18** **All or nothing**: every row is checked first by the rules of R2, R4 and R7 (the item exists in the company by code or barcode, it is stock managed, the quantity is valid, the item and stock type has no opening yet, the same item and stock type is not in the file twice). If any row is wrong, **nothing is imported** and the answer is a report of every wrong row with its row number and the reason (a message code); if every row is right, all openings are posted in one transaction and the answer says how many.
- **R19** An imported opening is the same entry as a typed one (R7): kind `OPENING`, the given date, a note naming the file, who and when. The file itself is not kept.

## 4. Data

Schema `stock`:
- `item_stock` (aggregate root, company-owned, platform P18): `company_id`, `item_id` (plain id, no foreign key across schemas), `main_quantity`, `damaged_quantity`, `sample_quantity` (integers; running totals of R6), plus the standard audit and concurrency columns. Unique on `(company_id, item_id)`. Parallel postings for one item queue on its concurrency token or a lock.
- `stock_entry` (insert-only child of `item_stock`, company-owned): `company_id`, `item_stock_id`, `item_id`, `stock_type`, `movement_kind`, `quantity_in`, `quantity_out` (integers, exactly one of them greater than zero), `movement_date` (`date`), `document_type`, `document_id`, `document_number`, `note`, `created_at`, `created_by`. Index on `(company_id, item_id, movement_date)`; unique on `(company_id, document_type, document_id, item_id, stock_type, movement_kind)` for document postings (R11).

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session (platform P19, P20); the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `GET stock` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the current stock list of R10 |
| `GET items/{itemId}/stock` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the item's three quantities and its entries newest first with the quantity after each (R10); `404` for an unknown item; `400` for an item that is not stock managed |
| `POST items/{itemId}/stock/opening` | `OWNER`, `SUPER_ADMIN` (entered) | `{stockType, quantity, openingDate}` -> `201`; `409` when the opening exists (R7) |
| `GET stock/opening/template` | `OWNER`, `SUPER_ADMIN` (entered) | the `.xlsx` template with the three columns (R17) |
| `POST stock/opening/import` | `OWNER`, `SUPER_ADMIN` (entered) | multipart: the file and `openingDate` -> `200` with the count imported, or `400` with the report of every wrong row (row number and code); nothing is imported when any row is wrong (R17, R18) |
| `POST items/{itemId}/stock/adjustment` | `OWNER`, `SUPER_ADMIN` (entered) | `{stockType, countedQuantity, reason}` -> `201` with the entry posted; `400` when nothing changes (R8) |

The posting capability of R11 is not an endpoint: it is called by the document modules inside their own requests. Errors are `ProblemDetails` with a message code from the module's catalogue. Validation failures are `400`.

## 6. User interface

Written in the UI plan; summary: in the company workspace (`member` app) the Owner and Staff get a "Stock" menu in its own group: the stock list with Main, Damaged and Sample quantities and a warning mark at zero or below; an item's stock opens beside the list (platform U10) with its history; the Owner enters the opening stock and adjustments there. Each state shows loading, empty, error with retry and success.

## 7. Events

Raised by the `ItemStock` aggregate (past tense, ids and values only): `StockMoved` (item, stock type, kind, quantity in, quantity out, the new quantity). Reactions: none in this feature (later: the dashboard's low-stock count).

## 8. Dependencies

`stock` depends on `iam` for the current company, on `core` and the platform (P18-P23), and on `item` through its directory capability ([specs/item.md](item.md) R16, R17). Purchase, School Invoice, Counter Sale, Return and Sample will depend on `stock` through the posting capability (R11).

## 9. Acceptance criteria

- **AC1** The Owner of company A enters an opening of 100 Main for book X dated 2026-04-01: `201`; X's Main quantity is 100; a second opening for X Main is `409`; an opening of 5 Damaged for X is accepted; an opening for a pen (not stock managed) is `400`; a quantity of 0, -3 or 2.5 is `400` (R2, R4, R7, R15).
- **AC2** An adjustment of X Main with counted 97 and reason "physical count" posts one entry `ADJUSTMENT` out 3 and the quantity becomes 97; counted 97 again is `400`; a reason of 3 characters is `400`; Staff adjusting is `403` (R8, R10).
- **AC3** The item's history lists the entries newest first with the quantity after each; the sum of in minus out per stock type equals the shown quantity (R4, R6, R10).
- **AC4** Posted through the capability (scratch caller): a document with lines X Main out 120 and a pen out 2 posts one entry for X (the pen is skipped), X's Main goes from 97 to -23 without refusal, and the answer lists X as at or below zero; posting the same document again is refused; a cancel posts one `REVERSAL` in 120 and X returns to 97 (R2, R6, R9, R11).
- **AC5** The Owner of B sees none of A's stock; asking for A's item stock answers `404`, identical to an unknown item; a `companyId` in the query or body changes nothing (R1).
- **AC6** The stock list is paged and capped at 200; search, the category, vendor and class filters, the "zero or below" filter and the sorts work; each row shows the three quantities and the warning mark (R9, R10).
- **AC7** A Super Admin with no company entered gets an empty list and a write answers `403 core.company_required`; after entering A they read and can enter an opening (R10).
- **AC8** Entries cannot be changed or deleted through any route; every entry shows who made it and when (R4, R13).
- **AC9** An Excel file with three right rows (a book by code, a copy by barcode, a book with stock type `DAMAGED`) and the date 2026-04-01 imports 3 openings; the stock list shows them; a second upload of the same file is refused with a report naming all three rows as "opening exists" and imports nothing (R17, R18, R19).
- **AC10** A file with one right row and rows for an unknown code, a pen, a quantity of 0, a quantity of 2.5 and the same item twice imports nothing and reports exactly those rows with their reasons; a `.csv`, a file above 5 MB and a file of 5001 rows are refused (R17, R18).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `stock` with its own schema. `ItemStock` (one per item) is a company-owned aggregate root keeping running totals; its entries are insert-only children. Postings go through the root, so a total and its entries always change together. (proposed by the assistant) |
| D2 | The document modules post through one capability of this module (R11) and never write entries themselves; a document posting is unique per document, item, stock type and kind, so a retry cannot count twice. (proposed by the assistant) |
| D3 | The stock entries are the history (R13); the platform audit trail is not used for them. (proposed by the assistant) |
| D5 | Excel files are read with the package ClosedXML (MIT licence), approved by the owner on 2026-11-14 for this import; the later imports (school, vendor, item, price) use the same package. (owner's answer to Q9) |
| D4 | Whether an item is stock managed is read from the item module's directory (item R17); the stock module never keeps its own copy of that flag. (proposed by the assistant) |

## 11. Open questions

None. Q1-Q10 were answered by the owner one by one on 2026-11-14: all with the recommended answers except Q9, answered "now" (Excel opening stock is built in this module with ClosedXML, R17-R19, D5); for the import the owner chose the three columns with one date per file, all or nothing with a report of every wrong row, and a limit of 5000 rows, 5 MB, `.xlsx` only. The list keeps the questions with the answer taken.

- **Q1** Quantities. Recommended: whole numbers only (books and copies are counted in pieces) (R4, R15). Alternative: decimals.
- **Q2** Opening stock. Recommended: at most one opening entry per item and stock type; later corrections are adjustments (R7). Alternative: allow several openings.
- **Q3** How an adjustment is entered. Recommended: the Owner types the **counted** quantity and the system posts the difference (R8). Alternative: the Owner types "+5" or "-3" directly.
- **Q4** What "low stock" means (FR-STK-06, the dashboard's "kam stock count"). Recommended: zero or below for now; a per-item minimum level comes with the Planning module. Alternative: a company-wide threshold (for example below 10).
- **Q5** What Staff may see. Recommended: the stock list and an item's history, read only (they need it to sell). Alternative: only the Main quantity, no history.
- **Q6** Cancelling a document's stock. Recommended: the document module asks this module to reverse exactly what that document posted (`REVERSAL` entries), never edits entries (R11). Alternative: the document module posts its own opposite movements.
- **Q7** Deactivated items. Recommended: they keep their stock and can still be adjusted; the list hides them unless a filter asks (R16).
- **Q8** Adjustments of Damaged and Sample stock. Recommended: allowed, same rules as Main (R8).
- **Q9** Excel opening stock (FR-STK-10). Answered: now, in this module (R17-R19), with ClosedXML (D5).
- **Q10** The Super Admin's stock screens in the `admin` app. Recommended: later, like the other masters.
