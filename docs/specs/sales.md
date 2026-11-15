# Spec: sales - School orders and invoices

- **Module:** `sales` (PostgreSQL schema `sales`)
- **Status:** R19-R27 and AC11-AC18 Approved by abhishek in chat on 2026-11-15 (the wholesale order questions answered one by one, all with the recommended answers); R1-R18 Approved by abhishek in chat on 2026-11-15 (Q1-Q14 answered one by one: Q1 per-session order numbers and Q3 a counter per company as the owner said, the rest with the recommended answers; D1-D3 with them); R34 and AC25 (the payment type, Cash/Credit memo) Approved by abhishek on 2026-11-15 (Q21 with the recommended answer); R28-R33 and AC19-AC24 (the invoice PDF) Approved by abhishek on 2026-11-15 (the owner's instructions of 2026-11-15: Chromium, a PDF template, GST as a total and not per book, terms and conditions, the header with the GSTIN and a Cash/Credit Memo "GST Exempted" title, one bank account per company, the logo, the full name and address, A4 with no wasted white space across many pages, the bank details at the bottom, and a drawn invoice format; Q15-Q20 with the recommended answers).
- **Last updated:** 2026-11-15 (the invoice PDF: R28-R33)
- **Source docs:** [BRD §2, §4.10 School Order and Invoice (FR-ORD-01 to FR-ORD-15), §5.1 BR-SES, §5.2 BR-PRC, §5.3 BR-DIS, §5.4 BR-GST, §5.5 the stock movement table, §5.6 BR-LED, §6 Documents, §8 Data Model (SalesOrder, Invoice) and design decisions 3 and 4, §9 Q-01, Q-02, Q-05](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P25, U14; [specs/school.md](school.md) R5, R16, R18, R19; [specs/item.md](item.md) R16; [specs/price.md](price.md) R4, R18; [specs/stock.md](stock.md) R11, R20; [specs/finance.md](finance.md) R5; [specs/company.md](company.md) R16-R20
- **Plans:** [sales#09](../plans/sales/2026-11-15-09-order-payment-type.md) (Approved, R31, R34, AC21, AC25), [sales#10](../plans/sales/2026-11-15-10-ui-order-payment-type.md) (Approved, UI), [sales#07](../plans/sales/2026-11-15-07-invoice-pdf.md) (In progress, R28-R33, AC19-AC24), [sales#08](../plans/sales/2026-11-15-08-ui-invoice-pdf.md) (Approved, R33), [sales#05](../plans/sales/2026-11-15-05-wholesale-orders.md) (Done), [sales#06](../plans/sales/2026-11-15-06-ui-wholesale-orders.md) (In progress), [sales#01](../plans/sales/2026-11-15-01-school-orders.md) (Done), [sales#02](../plans/sales/2026-11-15-02-ui-school-orders.md) (In progress), [sales#03](../plans/sales/2026-11-15-03-invoices.md) (Done), [sales#04](../plans/sales/2026-11-15-04-ui-invoices.md) (In progress); what it needs from other modules: [school#05](../plans/school/2026-11-15-05-session-of-a-date.md) (Done), [school#06](../plans/school/2026-11-15-06-ui-session-cut-offs.md), [price#05](../plans/price/2026-11-15-05-price-directory.md), [stock#05](../plans/stock/2026-11-15-05-stock-quantities.md), [company#06](../plans/company/2026-11-15-06-company-tax-profile.md) (all Approved)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`sales` holds what the company sells to schools: the **school order** (a draft that becomes a confirmed order) and the **invoices** made from it, possibly several for one order (partial delivery, Q-02). An invoice fixes its prices, discounts and GST when it is made (a snapshot, BR-PRC-03), takes the stock out through the stock module and debits the school's ledger through the ledger; it is never edited, only cancelled, which reverses both (BR-LED-05).

Feature **School order and invoice** (R1-R18). Later features of this module: counter sale (§4.11), school returns (§4.12), invoice from samples (§4.13).

Feature **Invoice PDF** (R28-R33, Draft): the printed A4 tax invoice of an issued invoice — the company's identity in the header, the lines, a rate-wise GST summary (the GST on the total, never a per-book column), the totals in words, the bank details, the terms and conditions and the signature, across as many pages as the lines need with the header repeated on each.

Out of scope for this feature: the invoice PDF and print layout of the **other** documents (counter receipt, credit note, ...: their own modules later); adding lines from the class book list (FR-ORD-02, the Planning module); payments and their allocation to invoices (the Payment module); attachments on documents (owner's note, backlog); the Super Admin's screens in the `admin` app.

## 2. Concepts and glossary

- **Order**: a school's list of items and quantities for one session, first a **draft**, then **confirmed**; it is **closed** when nothing more will be invoiced from it.
- **Line**: one item of an order or invoice: the chosen **price version** (its name, MRP and rate), the quantity and the **item discount**.
- **Item discount / total discount**: a percentage or a rupee amount, on a line or on the whole document (BR-DIS-01, BR-DIS-02).
- **Taxable value**: a line's amount after its item discount and its share of the total discount (BR-DIS-03, BR-DIS-04).
- **Invoice**: what is delivered and billed now, from a confirmed order; numbered per financial year; **cancelled** but never edited or deleted.
- **Remaining quantity**: a line's ordered quantity minus what its live (not cancelled) invoices hold.

## 3. Business rules

**School order and invoice**
- **R1** Orders and invoices belong to exactly one company, the company of the session (platform P18-P21). Schools, items, prices, stock and the ledger are reached only through their modules' capabilities (school R16, R18; item R16; price R18; stock R11, R20; finance R5; company R16), never by joining their tables.
- **R2** An **order** has: a generated **number** per company and per **session**: `ORD/2027-28/0001` (a Jan-Dec school's session gives `ORD/2027/0001`), starting again at 0001 for each session, from the company code series with the prefix `ORD/<session>/` (platform P23, Q1). Orders for the next session are made before its financial year starts (February-March, BR-SES-04), so the number follows the order's session, not its date. The **school** (an active school of the company), the **order date** (the business date by default, not in the future), the **session** (school R18: filled from the school and the order date; the user may choose the session before or after it, BR-SES-06, Q9), **GST applied** (yes or no, starting from the school's GST default, school R5, BR-GST-03), the lines, the **total discount** and a remark (at most 500 characters).
- **R3** An order **line** has: the **item** (active), the chosen **price version** of that item (price R18; the item's default version on the order date unless the user chooses another, BR-PRC-02), and from that version the **MRP** and the **rate** (the version's sale price, Q4), the **quantity** (a whole number from 1), and the **item discount**: none, a percentage (0-100, two decimals) or a rupee amount (at least zero, at most the line's rate times quantity). One line per item and price version; adding the same item again adds 1 to its quantity (FR-ORD-03). At most 500 lines (FR-ORD-02). An item with no price version cannot be added (Q13).
- **R4** Items are added by name or code search or by **barcode** (item R16); a barcode that matches no item answers "item not found" and adds nothing (FR-ORD-04).
- **R5** **Low stock never blocks** (FR-ORD-08, stock R9): a line of a stock-managed item whose Main quantity (stock R20) is below the line's quantity shows a yellow warning; the order and the invoice are never refused for it.
- **R6** **Order lifecycle** (FR-ORD-09, Q2, Q11): a `DRAFT` can be saved at any point, changed freely and deleted. **Confirm** needs at least one line and makes it `CONFIRMED`; a confirmed order's lines, prices and discounts no longer change. A confirmed order becomes `CLOSED` when every line's remaining quantity is zero, or when the Owner closes it with a reason (5-500 characters), after which nothing more is invoiced from it.
- **R7** **The amounts** of an order and of an invoice are computed by one rule (BR-DIS-03 to BR-DIS-05, BR-GST-04, BR-GST-05), every step rounded to 2 decimals **half away from zero** (Q5):
  1. line gross = rate × quantity; item discount = gross × percentage / 100, or the rupee amount; line amount = gross − item discount;
  2. subtotal = the sum of the line amounts; total discount = subtotal × percentage / 100, or the rupee amount (at most the subtotal);
  3. the total discount is shared over the lines in proportion to their line amounts (BR-DIS-04); the paise left over by the rounding go to the line with the largest amount (the first such line on a tie); taxable value = line amount − share;
  4. with GST applied, each line's GST is on its taxable value at the item's GST rate, **exclusive** (on top, Q-01): when the company's state equals the school's state, CGST = SGST = taxable × rate / 2 / 100 each rounded; otherwise IGST = taxable × rate / 100 rounded (BR-GST-05); a missing state counts as the same state (Q6); without GST every GST amount is zero and the invoice shows no GST columns (BR-GST-06);
  5. total before rounding = the sum of the taxable values + all GST; **grand total** = rounded to the nearest rupee, half away from zero; round-off = grand total − total before rounding (Q7).
- **R8** An **invoice** is made from a `CONFIRMED` order (FR-ORD-10, FR-ORD-11, Q2): the user enters, for each line, the quantity delivered now (by default its remaining quantity; never more than it; at least one line above zero). The invoice copies the order's school, session and GST choice and each line's item code, name, HSN, GST rate, price version name, MRP, rate and item discount; a percentage discount applies as it is, a rupee discount (item or total) is shared by the delivered quantity (item) or by the invoice's share of the order subtotal (total), rounded as R7, and the last invoice that empties the order takes what is left (Q10). An invoice whose grand total is not above zero is refused.
- **R9** The invoice **date** is the business date by default; it may be earlier, never in the future and never before the order date (Q8). Its **financial year** is the April-March year of that date (finance R4).
- **R10** The invoice **number** is per company and per financial year: `INV/2026-27/0001`, `INV/2026-27/0002`, ..., starting again at 0001 each financial year, from the company code series (platform P23) with the prefix `INV/<financial year>/` (Q3). Every company has its own counter. A number is never given twice, also not after a cancel; a cancelled invoice keeps its number. An invoice made in February-March for the next session's order still takes the financial year of its date (BR-SES-02).
- **R11** **Making an invoice** posts, in the same request transaction: the stock out of every stock-managed line (stock R11: Main, `SCHOOL_INVOICE`, the delivered quantity; non-stock items are skipped) and one ledger transaction (finance R5, BR-LED-01): debit `SCHOOL_RECEIVABLE` (the school) the grand total; credit `SALES` the sum of the taxable values; credit `GST_OUTPUT` the sum of the GST; and the round-off on `ROUND_OFF` (credit when it is positive, debit when negative); the document is the invoice, its date the invoice date, its session the order's. The answer lists the items whose Main stock went to zero or below (R5).
- **R12** An invoice is **never edited** (FR-ORD-14). The Owner **cancels** it with a reason (5-500 characters): the stock and ledger postings are reversed (stock R11, finance R5) dated the business date of the cancel, the invoice becomes `CANCELLED` and keeps its number, and its quantities count again as remaining on the order (a closed order that becomes open again goes back to `CONFIRMED`, unless the Owner had closed it with a reason).
- **R13** **Snapshot** (FR-ORD-12, BR-PRC-03, BRD §8 decision 3): an invoice keeps the school's name, code, state and GSTIN, the company's state and every line's values as they were when it was made; a later change of a price, an item or the school changes no invoice.
- **R14** **Who does what** (Q3 of company D5, Q11 here): the Owner, Staff and an entered Super Admin create, change, confirm and delete draft orders and make invoices; only the Owner (and an entered Super Admin) closes an order and cancels an invoice. Anonymous callers get `401`.
- **R15** **Reading**: the orders and the invoices of the company, paged (page size capped at 200), searchable by number and school name or code, filterable by school, status and session, newest first; each school's profile shows its orders and invoices (platform U14, Q11). An order shows its lines with the remaining quantities and its invoices; an invoice shows its lines, discounts, GST, round-off, grand total and its money story (made, cancelled).
- **R16** Every create and change of an order or invoice is recorded who and when (the standard audit columns and the audit trail of `core`); the postings are the stock entries and ledger transactions themselves.
- **R17** Money is `numeric(18,2)`, percentages `numeric(5,2)`, never float; the rounding is R7's and happens only in the domain of this module.
- **R18** Every input is checked at the API boundary (lengths, ranges, at most 500 lines, whole quantities, two decimals) and the rules again in the domain; an order or invoice of another company is `404`, identical to an unknown id.

**Wholesale orders** (owner, 2026-11-15: "single single entry bahut tough hoga")
- **R19** **Orders menu**: the company app has a BUSINESS parent **Orders** with the order list (all schools, filters school, status, session, search) and the invoice list; "New order" first asks for the school, then opens the order editor. A school's profile keeps its Orders and Invoices tabs (the same documents). This changes platform U14 for orders and invoices.
- **R20** **A confirmed order can be changed** until it is closed: lines can be added; a line's quantity can be raised or lowered but never below its invoiced quantity; a line with nothing invoiced can be deleted and its price version and item discount changed; a line with something invoiced keeps its price version and item discount; the total discount and the GST choice can be changed only while nothing is invoiced. Every change recomputes the order (R7) and is audited (R16). An order whose remaining quantity becomes zero after a change closes (R6).
- **R21** Each order line may carry a **class** (one of the school's classes, school R6): the same item for two classes is two lines, so packing and later class-wise invoices follow the class; a line without a class is the school's general need. "One line per item and price version" of R3 becomes one line per item, price version and class.
- **R22** **Add class sets** (FR-PLN-01, owner's answers): on a draft or confirmed order the user chooses classes and the number of students of each (prefilled with the class's student estimate, planning R3); every book of each class's list of the order's session (planning R2) is added as a line of that class with quantity = students x quantity per student, at the item's default price version on the order date; an item with no price version or no longer active is left out and reported; a line of the same item, version and class already on the order is increased instead of doubled.
- **R23** **Bulk add**: many items chosen at once (filters: category, class, publisher/vendor, subject, search) are added with one quantity and an optional class, each at its default price version; the rules of R3 and R21 apply to each.
- **R24** Lines are edited in place on the order (quantity, class, version, discount, delete) with keyboard-friendly entry; the server recomputes after each save (R7). An order holds at most 500 lines (R3).
- **R25** Every refusal names the line it is about (item code and class) so a long order can be corrected quickly.
- **R26** **Invoice layout** (owner, 2026-11-15: "group kar ke dikha sakte hai ya kewal book ka list, both option"): an invoice line keeps the class of its order line. The same invoice is shown and printed in one of two layouts, without changing its data, numbers or totals: **by class** (lines grouped under each class with a class subtotal; lines without a class under "General") or **book list** (no classes; lines of the same item, price version, rate and item discount are shown as one line with the summed quantity and amounts; lines that differ in any of these stay separate). Each school has a default layout kept by this module (initially book list), set by the Owner from the school's profile; the invoice screen opens in the school's default and can be switched there and then.
- **R27** **Easy new order**: New order asks only for the school (search by name or code); the date is today, the session and GST come from the school (R2, R7) and can be changed; the first screen offers the three ways to add books side by side (class sets R22, bulk add R23, scan or search R24), and the order can be saved as a draft at any time.

**Invoice PDF** (Approved by abhishek, 2026-11-15)
- **R28** An issued invoice can be handed to the user as a **PDF** (FR-ORD-10, §6): the document is built as **HTML + CSS** from a template that lives in this module and rendered by the platform's shared renderer (platform P25, a headless Chromium). It is rendered **on demand** from the invoice and its snapshot (R13) and is **never stored**; a cancelled invoice prints too, marked `CANCELLED` (Q18).
- **R29** The PDF follows the invoice's **layout** (R26): by class with class subtotals, or book list; the layout chosen on the invoice screen is the one printed, and the totals are identical in both.
- **R30** **Page**: A4 portrait, as many pages as the lines need, and **no wasted white space**: lines pack about 48 to a page (a 300-line school invoice must not run to hundreds of half-empty pages). The company header (logo, full name, address, GSTIN when R20 allows, phone, email) and the column header repeat on **every** page, and a slim footer on every page carries the invoice number and `Page x of y` (FR-ORD-15). The party block is on the first page only; the totals, the bank details, the terms and the signature are at the bottom of the **last** page (R33), so the pages before it are full of lines (the owner: the account details are shown at the bottom). The **closing block** (rate-wise GST summary, totals, amount in words, bank details, terms and signature) is **never split**: when it does not fit under the last lines it moves whole to one more page that repeats the company header and the invoice number, so the bank details and the terms are never lost, cut or separated from the totals (owner's question, 2026-11-15).
- **R31** **Title and header**: the title is **TAX INVOICE** when the invoice has GST applied; when it does not it is **CASH MEMO** (payment type `CASH`) or **CREDIT MEMO** (`CREDIT`, R34), each with a `GST Exempted` note (BR-GST-06) (Q16). The header carries the company's GSTIN (when R20 allows) and PAN. Under the title: the invoice number, the invoice date, the order number and session. The party block carries the school's name, code, address, state and GSTIN (from the snapshot, R13) and the payment type.
- **R32** **Lines and GST**: the lines show item code, name, HSN, class (when the order line carried one), quantity, rate, discount and amount. There is **no per-line GST column** (the owner: "GST total ka hota hai, per book nahi"); the GST is shown once as a **rate-wise summary** — for each HSN/rate, the taxable value and CGST + SGST (or IGST) and the tax — followed by subtotal, total discount, taxable value, total tax, round-off, **grand total** and the **amount in words** (Indian numbering, rupees and paise). The computation is unchanged (R7 computes each line at its item's rate; the summary only adds those up) (Q15).
- **R33** **Bottom**: the invoice prints the company's **bank account** (R19: account holder, bank, account number, IFSC, branch, UPI id), the **terms and conditions** and the footer line (R20), and the **signature image** (R18) above a signatory line for the company (FR-CMP-02/03, §6). The invoice **screen** shows the same rate-wise GST summary, totals and amount in words as the PDF, so the detail on screen and the print never disagree, and offers **Print** and **Download PDF** (sales#08).
- **R34** **Payment type** (owner, 2026-11-15, Cash/Credit memo): an order has a payment type, `CASH` or `CREDIT`, that starts from the school's default (school R20) and can be changed on a draft and on a confirmed order until something is invoiced (the same lock as the total discount, R20); an invoice keeps its order's payment type (R13). It decides the title of a GST-off invoice (R31) and the "Payment" line of the party block.

## 4. Data

Schema `sales` (all company-owned, platform P18):
- `school_order` (aggregate root): `number`, `school_id`, `order_date`, `session`, `is_gst_applied`, `total_discount_type` (`NONE`, `PERCENT`, `AMOUNT`), `total_discount_value`, `remark`, `status` (`DRAFT`, `CONFIRMED`, `CLOSED`), `close_reason`, the computed totals (subtotal, total discount, taxable, GST, round-off, grand total), audit and concurrency columns. Unique `(company_id, number)`.
- `school_order_line` (child): `item_id`, `price_version_id`, item code, name, HSN and GST rate, price version name, `mrp_amount`, `rate_amount`, `quantity`, `item_discount_type`, `item_discount_value`, the computed amounts, `invoiced_quantity`.
- `invoice` (aggregate root): `number`, `financial_year`, `invoice_date`, `school_order_id`, school snapshot (id, code, name, state, GSTIN), company state, **company snapshot** (name, address, GSTIN and the printed bank account, terms and footer as they were when it was made, R13/R33; the logo and signature are read live from the company, R17/R18), `session`, `is_gst_applied`, `payment_type` (R34), the discounts, every total, `status` (`ISSUED`, `CANCELLED`), `cancel_reason`, `cancelled_at`, audit and concurrency columns. Unique `(company_id, number)`.
- `invoice_line` (child): the order line id, every copied value of R8 and its computed amounts (gross, item discount, line amount, total discount share, taxable, CGST, SGST, IGST).

No new table for the invoice PDF: it is rendered on demand from the invoice and its snapshot (R28).

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session; the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `POST school-orders` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | a new draft `{schoolId, orderDate?, session?, isGstApplied?, lines?, totalDiscount?, remark?}` -> `201` with the number and the computed totals |
| `PUT school-orders/{id}` | same | replaces a draft's values and lines; `409` when not a draft |
| `DELETE school-orders/{id}` | same | a draft only (soft delete); `409` otherwise |
| `POST school-orders/{id}/confirm` | same | R6 |
| `POST school-orders/{id}/close` | `OWNER`, `SUPER_ADMIN` (entered) | `{reason}` R6 |
| `GET school-orders/session-choice?schoolId=&date=` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the school's session on a date and the ones before and after it (school R18), for the order screen's choice (R2) |
| `GET school-orders`, `GET school-orders/{id}` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R15; the detail has the lines with remaining quantities, the stock warning of R5 and the order's invoices |
| `POST school-orders/{id}/invoices` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | `{invoiceDate?, lines: [{orderLineId, quantity}]}` -> `201` with the invoice and the items at or below zero (R8-R11) |
| `POST invoices/{id}/cancel` | `OWNER`, `SUPER_ADMIN` (entered) | `{reason}` R12 |
| `GET invoices`, `GET invoices/{id}` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R15 |
| `GET invoices/{id}/pdf` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | the invoice as a PDF (R28-R33); `?layout=class\|booklist` overrides the school's default (R26); `404` for another company's invoice; a cancelled invoice prints marked `CANCELLED` |

Errors are `ProblemDetails` with a message code from the module's catalogue.

## 6. User interface

Written in the UI plans; summary (platform U14, owner's document pattern): the school's profile gets the tabs **Orders** and **Invoices**; "New order" opens the order editor as a full-width page (an order holds 100-200 lines, Q11) with barcode and search entry, the price version per line, discounts, the GST toggle, live totals and the yellow stock warning; a confirmed order shows "Make invoice" (quantities per line, remaining by default). An invoice opens with a header showing its money story (ordered, invoiced, cancelled) and its lines, discounts, GST, round-off and grand total; the Owner cancels it with a reason. Required fields carry a red `*`; each state shows loading, empty, error with retry and success.

**Invoice screen and PDF** (sales#08): the invoice detail shows the GST as the **rate-wise summary** of R32 (never a per-line GST column), the totals and the amount in words, exactly as the PDF, with **Print** and **Download PDF** actions.

**The drawn A4 invoice** (the owner's instruction of 2026-11-15; `Page x of y` and the header repeat on every page, the block below the lines appears once at the end):

```text
+-----------------------------------------------------------------------------------------------+
| [LOGO]  CHILDREN BOOK CENTER                                     TAX INVOICE  (GST applied)     |
|         12, Station Road, Civil Lines, Kanpur, U.P. - 208001          -- or --                |
|         Ph: 0512-2233445 | cbc@example.com                    CASH / CREDIT MEMO              |
|         GSTIN: 09ABCDE1234F1Z5   State: Uttar Pradesh (09)      GST Exempted                   |
+-----------------------------------------------------------------------------------------------+
| Invoice No.: INV/2026-27/0001                          Invoice Date: 15-11-2026               |
| Order No.:   ORD/2026-27/0042                          Session: 2026-27                       |
| Bill To:     ST. MARY'S CONVENT SCHOOL                                                         |
|              Code: SCH-0007   Address: 5 Mall Road, Kanpur   State: UP (09)                    |
|              GSTIN: 09ZZZZZ9999Z9Z9                                                            |
+----+------------+----------------------------+-------+------+--------+-------+-----------------+
| S. | Item code  | Description                | Class | Qty  | Rate   | Disc% | Amount          |
+----+------------+----------------------------+-------+------+--------+-------+-----------------+
|  1 | BOK-0451   | Mathematics - 8            |   8   |  40  | 250.00 |  5%   |  9,500.00       |
|  2 | BOK-0452   | Science - 8                |   8   |  40  | 260.00 |  --   | 10,400.00       |
|  3 | BOK-0453   | Atlas                      |   5   |  38  | 120.00 |  --   |  4,560.00       |
|  ... (as many lines as the invoice holds; several to a page, no blank half pages)              |
+----+------------+----------------------------+-------+------+--------+-------+-----------------+
| GST Summary (rate-wise; NOT per line)                                                          |
|   HSN     Rate      Taxable Value          CGST        SGST        IGST        Total Tax         |
|   4901     0%         19,350.00               -           -           -             -           |
|   4901    12%          3,420.00          205.20      205.20           -         410.40          |
|   Total                                                                            410.40      |
+-----------------------------------------------------------------------------------------------+
|  Subtotal ............................................... 22,770.00                            |
|  Total Discount (2%) .....................................    455.40                            |
|  Taxable Value ........................................... 22,314.60                            |
|  Total Tax (CGST 205.20 + SGST 205.20) ...................    410.40                            |
|  Round Off ...............................................     +0.00                            |
|  GRAND TOTAL ............................................. 22,725.00                            |
|  Amount in words: Rupees Twenty-Two Thousand Seven Hundred Twenty-Five Only                     |
+-----------------------------------------------------------------------------------------------+
|  Bank: HDFC Bank  |  A/c Name: Children Book Center  |  A/c No: 5010xxxxxxxx1234                 |
|  IFSC: HDFC0000123  |  Branch: Civil Lines, Kanpur  |  UPI: cbc@hdfcbank                       |
|  Terms & Conditions: 1. Goods once sold will not be taken back.  2. ...  3. ...                 |
+-----------------------------------------------------------------------------------------------+
|  For CHILDREN BOOK CENTER                                   [ SIGNATURE IMAGE ]                 |
|                                                             Authorised Signatory                |
|  Page 1 of 3                                                        INV/2026-27/0001            |
+-----------------------------------------------------------------------------------------------+
```

## 7. Events

Raised by the aggregates (past tense, ids and values only): `SchoolOrderConfirmed`, `SchoolOrderClosed`, `InvoiceIssued` (invoice id, school, grand total), `InvoiceCancelled`. Reactions: none in this feature (the postings are made in the same use case through the capabilities, R11, R12).

## 8. Dependencies

`sales` depends on `school` (directory R16, session R18), `item` (directory R16), `price` (directory R18), `stock` (posting R11, quantities R20), `finance` (posting R5), `company` (tax profile R16 and invoice identity R17-R20), and the platform (P18-P25). Payment and returns will depend on `sales` by the invoice id.

## 9. Acceptance criteria

- **AC1** A draft for school S (Apr-Mar, same state as the company) dated 2026-11-15 gets session `2026-27` and `ORD/2026-27/0001`, GST from S's default; it saves with no lines and is deleted while a draft; an order dated 2027-02-10 gets session `2027-28` and `ORD/2027-28/0001`; B's first order of 2026-27 also starts at 0001 (R2, R6).
- **AC2** Worked example (computed by hand before the code, R7): lines A book (GST 0%) rate 250.00 × 10 with item discount 5%, B copy (GST 12%) 45.50 × 7 with item discount ₹10.00, C pen (GST 18%) 12.00 × 3; total discount 2%; GST applied, same state -> line amounts 2375.00, 308.50, 36.00; subtotal 2719.50; total discount 54.39 shared 47.50, 6.17, 0.72; taxable 2327.50, 302.33, 35.28; CGST = SGST: B 18.14, C 3.18; GST 42.64; before rounding 2707.75; grand total 2708.00; round-off +0.25. Other state: IGST B 36.28, C 6.35; GST 42.63; before rounding 2707.74; grand total 2708.00; round-off +0.26. GST off: grand total 2665.11 -> 2665.00, round-off -0.11.
- **AC3** Invalid inputs are `400`: quantity 0 or 2.5, a percentage above 100, a rupee item discount above the line gross, a total discount above the subtotal, an inactive item, an item with no price version, 501 lines, an order date in the future; adding the same item and version again increases its quantity (R3, R18).
- **AC4** Confirm with no lines is `400`; a confirmed order cannot be changed or deleted (`409`); the Owner closes it with a reason; Staff closing is `403` (R6, R14).
- **AC5** An invoice of the example order delivering everything on 2026-11-15 gets `INV/2026-27/0001`, grand total 2708.00; SQL: stock entries `SCHOOL_INVOICE` out 10 (A) and 7 (B) and none for the pen, and one ledger transaction Dr S 2708.00 = Cr SALES 2665.11 + Cr GST_OUTPUT 42.64 + Cr ROUND_OFF 0.25; the order is `CLOSED`; the answer lists A when its Main stock went to zero or below (R8-R11, R5).
- **AC6** Partial delivery: invoicing 4 of A then 6 of A gives two invoices; a ₹ discount is shared and the last invoice takes the rest, and the two invoices together equal the order's totals; a further invoice of the now closed order is refused (`409`) (R8).
- **AC7** The first invoice dated 2027-04-01 gets `INV/2027-28/0001`; an invoice date after today or before the order date is `400` (R9, R10).
- **AC8** The Owner cancels an invoice with a reason: SQL shows the stock reversal and the opposite ledger transaction; the invoice keeps its number and is `CANCELLED`; its quantities are remaining again; cancelling twice is `409`; Staff cancelling is `403` (R12, R14).
- **AC9** Changing the price version's sale price, the item's name or the school's name after the invoice changes nothing on the invoice (R13).
- **AC10** The Owner of B gets `404` for A's order and invoice, identical to unknown ids; lists are paged, searchable and filtered; Staff see orders and invoices; anonymous `401` (R1, R14, R15, R18).
- **AC11** The menu shows BUSINESS > Orders with Order list and Invoices; New order asks for the school first (R19).
- **AC12** On a confirmed order with line A 10 (4 invoiced): A -> 6 is accepted, A -> 3 is `400`; deleting A is `409`; adding C and deleting C before any invoice are accepted; changing A's version is `409`, changing B's (nothing invoiced) is accepted; the totals are recomputed; lowering every remaining to zero closes the order (R20).
- **AC13** Class 4 dictionary 40 and Class 5 dictionary 38 are two lines with their classes; a class the school does not have is `400` (R21).
- **AC14** Adding the sets of Class 4 (45 students) and Class 5 (40) from the 2026-27 lists (Class 4: X 1, Y 2; Class 5: X 1, Z 1) adds X 45, Y 90 (Class 4) and X 40, Z 40 (Class 5); adding Class 5 again raises X and Z to 80; a book without a price is left out and reported (R22).
- **AC15** Bulk add of 12 ticked items with quantity 30 and class 6 adds 12 lines of 30 for Class 6 (R23).
- **AC16** A refused save names the line (item code and class) it is about (R25).
- **AC17** An invoice of Class 4 dictionary 40 and Class 5 dictionary 38 (same version, rate and discount) and Class 5 atlas 38: by class shows Class 4 (dictionary 40) and Class 5 (dictionary 38, atlas 38) with subtotals; book list shows dictionary 78 and atlas 38; both layouts give the same grand total; a school whose default is by class opens in by class; Staff cannot change the default (`403`) (R26).
- **AC18** New order with only a school chosen saves a draft with today's date, the school's session and GST choice (R27).
- **AC19** `GET invoices/{id}/pdf` of an issued invoice answers `200` with `application/pdf`; the bytes start with `%PDF` and open as a valid A4 document; a cancelled invoice prints with the `CANCELLED` mark; anonymous is `401`; the Owner of B gets `404` for A's invoice (R28, R29, R18).
- **AC20** A 300-line invoice renders compactly to a few pages (about 48 lines a page), not one line per page; the company header and `Page x of y` appear on every page, and the last page carries the totals, the amount in words, the bank details and the terms (R30). When the lines of an invoice fill its last page exactly, the closing block is printed whole on one more page with the header repeated, never cut across two pages and never dropped (R30).
- **AC21** With GST applied the title is `TAX INVOICE`; with GST off it is `CASH MEMO` for payment type `CASH` and `CREDIT MEMO` for `CREDIT`, each with the `GST Exempted` note and no CGST/SGST/IGST amounts in the PDF (R31, R34, BR-GST-06).
- **AC22** The PDF has no per-line GST column; its rate-wise summary lists one row per rate with the taxable value and the tax, and the tax total equals the invoice's total tax (R32).
- **AC23** The PDF's bottom carries the company's bank account, terms and conditions and signature image; a company with no signature prints an empty signatory line and still answers `200` (R33, R20).
- **AC24** The invoice screen shows the same rate-wise GST summary, totals and amount in words as the PDF, and its Print and Download PDF actions produce the same document (R33).
- **AC25** For a school whose default is `CASH`, a new order starts as `CASH`; changing it to `CREDIT` before any invoice is accepted, and the invoice made from it prints `CREDIT MEMO` (GST off); after that invoice, changing the payment type is `409 sales.totals_locked`; the invoice keeps `CREDIT` after the school's default changes (R34, R13).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `sales` with its own schema for school orders and invoices (later counter sale and returns). `SchoolOrder` and `Invoice` are separate company-owned aggregates; an invoice refers to its order by id and the order keeps the invoiced quantities. (proposed by the assistant, Q1) |
| D2 | The amounts of R7 are one domain rule used by the order and the invoice, so an order's preview and its invoice can never compute differently. (proposed by the assistant) |
| D3 | Making and cancelling an invoice post stock and ledger through their capabilities in the same use case and transaction (stock R11, finance R5), not through events, because the invoice must fail as a whole when a posting fails. (proposed by the assistant) |
| D5 | Owner, 2026-11-15: invoice layout by class or book list; book list merges the same item, version, rate and discount into one line; a per-school default plus a switch on the invoice screen. The layout is a view only: one invoice, one number, one total. |
| D4 | Owner, 2026-11-15: wholesale order entry: an Orders parent menu, confirmed orders editable above what is invoiced, class on each line, class sets from the planning module's class book lists, bulk add. Platform U14 is changed for orders and invoices accordingly. |
| D6 | The invoice PDF is built as **HTML + CSS** from a template kept in this module and rendered by the platform's shared **headless-Chromium** renderer (P25); it is rendered **on demand** and never stored. Answers Q12. (proposed by the assistant for the owner's instructions of 2026-11-15) |
| D7 | Owner, 2026-11-15: the printed and on-screen GST is a **rate-wise summary on the total**, never a per-line (per book) column; the computation is unchanged (R7 still computes each line at its item's rate, the summary adds them up). The invoice's `company snapshot` (name, address, GSTIN, bank account, terms, footer) is fixed when the invoice is made so a reprint never changes (R13, R33). Answers Q15, Q16, Q17. (owner's instruction "GST total ka hota hai, per book nahi", with the recommended answers) |
| D8 | Owner, 2026-11-15: whether a GST-off invoice is a Cash Memo or a Credit Memo is a **payment type on the order**, starting from the school's default (school R20) and kept on the invoice (R34). The account details, terms and signature are printed once, at the bottom of the last page, not on every page (R30). Answers Q21. (recommended answer; the owner's instruction on the A4 format) |

## 11. Open questions

None open from Q1-Q14 (all answered by the owner on 2026-11-15; the list below keeps the questions with the answer taken). The invoice PDF (Draft, 2026-11-15) adds Q15-Q20.

- **Q1** Order number. Answered by the owner: per session, `ORD/2027-28/0001`, restarting at 0001 every session, each company its own counter; orders for the next session start before its financial year (R2).
- **Q2** Order and invoice. Recommended: draft -> confirmed order; invoices are made from it for the quantities delivered now (partial delivery, Q-02); the order closes when all is invoiced (R6, R8). Alternative: confirming the order makes one invoice of everything at once.
- **Q3** Invoice number. Answered: per financial year of the invoice date `INV/2026-27/0001`, restarting each year, each company its own counter (R10).
- **Q4** The rate on a line. Recommended: the chosen price version's sale price, not typed; changes go through the discount (R3). Alternative: the user may type a rate.
- **Q5** Rounding. Recommended: every 2-decimal step half away from zero; the paise left by sharing the total discount go to the largest line (R7).
- **Q6** GST state. Recommended: same state -> CGST + SGST each half; other state -> IGST; a missing company or school state counts as same state (R7). Alternative: refuse GST until both states are filled.
- **Q7** Round-off. Recommended: the grand total to the nearest rupee, half away from zero, with a round-off line (R7, BR-DIS-05).
- **Q8** Invoice date. Recommended: today by default; may be earlier, never in the future, never before the order date (R9).
- **Q9** Session of an order. Recommended: filled from the school and the order date (school R18); the user may pick only the session before or after it (R2).
- **Q10** Rupee discounts on a partial invoice. Recommended: shared by quantity (item) or by the invoice's share of the subtotal (total); the last invoice takes what is left (R8). Alternative: discounts are typed again on each invoice.
- **Q11** Screens and roles. Recommended: orders and invoices are tabs of the school profile; the order editor is a full-width page (100-200 lines); Staff create, confirm and invoice; only the Owner closes orders and cancels invoices (R14, R15).
- **Q12** Invoice PDF (FR-ORD-10, FR-ORD-15, §6). Answered by the owner's instructions of 2026-11-15: build it now, as HTML + CSS rendered by a headless Chromium (platform P25), on demand and never stored; the format is the drawn A4 invoice of §6 (D6, D7).
- **Q13** An item with no price version. Recommended: it cannot be added to an order (R3).
- **Q14** Closing an order early. Recommended: the Owner closes a confirmed order with a reason when the rest will not be delivered (R6).

Open questions of the invoice PDF (answered by abhishek on 2026-11-15 with the recommended answers; the answers are D6/D7):

- **Q15** GST on the invoice: shown as a per-line column or as a rate-wise total? Answered: **rate-wise total, no per-line column** (the owner's instruction); the computation stays R7 (per line at the item's rate). Alternative: also change the computation to a single total rate. (R32, D7)
- **Q16** The title when GST is off (BR-GST-06): answered **CASH MEMO** or **CREDIT MEMO** (R34) with a `GST Exempted` note, and **TAX INVOICE** when GST is applied; alternative: always `TAX INVOICE`. (R31)
- **Q17** A reprint after the company changes its bank account or GSTIN: answered the invoice **snapshots the company's printed identity** (name, address, GSTIN, bank account, terms, footer) when it is made, so a reprint never changes; the logo and signature stay live (cosmetic). Alternative: always the company's current identity. (R13, R33, D7)
- **Q18** PDF stored or on demand: answered **on demand, never stored** (no file storage, no stale files, a cancelled invoice reprints with its mark); alternative: stored once at issue. (R28, D6)
- **Q19** The template: answered **Scriban** (already in the solution) over an HTML + CSS template kept in this module, the CSS printing the repeated header/footer and `Page x of y` through the renderer's options; alternative: build the HTML in C#. (R28, P25)
- **Q20** UPI QR (§6 says "bank details aur UPI QR"): answered the bank details and the **UPI id as text** in Phase 1, the scannable QR a later small addition; alternative: generate the UPI QR now. (R33)
- **Q21** How is it decided whether a GST-off invoice is a Cash Memo or a Credit Memo? Answered: an order **payment type** that starts from the school's default and can be changed until something is invoiced; alternatives: only a school field, or chosen when the invoice is made. (R31, R34, D8)
