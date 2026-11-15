# Spec: sales - School orders and invoices

- **Module:** `sales` (PostgreSQL schema `sales`)
- **Status:** R1-R18 Approved by abhishek in chat on 2026-11-15 (Q1-Q14 answered one by one: Q1 per-session order numbers and Q3 a counter per company as the owner said, the rest with the recommended answers; D1-D3 with them)
- **Last updated:** 2026-11-15
- **Source docs:** [BRD §2, §4.10 School Order and Invoice (FR-ORD-01 to FR-ORD-15), §5.1 BR-SES, §5.2 BR-PRC, §5.3 BR-DIS, §5.4 BR-GST, §5.5 the stock movement table, §5.6 BR-LED, §6 Documents, §8 Data Model (SalesOrder, Invoice) and design decisions 3 and 4, §9 Q-01, Q-02, Q-05](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P24, U14; [specs/school.md](school.md) R5, R16, R18, R19; [specs/item.md](item.md) R16; [specs/price.md](price.md) R4, R18; [specs/stock.md](stock.md) R11, R20; [specs/finance.md](finance.md) R5; [specs/company.md](company.md) R16
- **Plans:** [sales#01](../plans/sales/2026-11-15-01-school-orders.md) (Done), [sales#02](../plans/sales/2026-11-15-02-ui-school-orders.md) (Approved), [sales#03](../plans/sales/2026-11-15-03-invoices.md) (Done), [sales#04](../plans/sales/2026-11-15-04-ui-invoices.md) (Approved); what it needs from other modules: [school#05](../plans/school/2026-11-15-05-session-of-a-date.md) (Done), [school#06](../plans/school/2026-11-15-06-ui-session-cut-offs.md), [price#05](../plans/price/2026-11-15-05-price-directory.md), [stock#05](../plans/stock/2026-11-15-05-stock-quantities.md), [company#06](../plans/company/2026-11-15-06-company-tax-profile.md) (all Approved)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`sales` holds what the company sells to schools: the **school order** (a draft that becomes a confirmed order) and the **invoices** made from it, possibly several for one order (partial delivery, Q-02). An invoice fixes its prices, discounts and GST when it is made (a snapshot, BR-PRC-03), takes the stock out through the stock module and debits the school's ledger through the ledger; it is never edited, only cancelled, which reverses both (BR-LED-05).

Feature **School order and invoice** (R1-R18). Later features of this module: counter sale (§4.11), school returns (§4.12), invoice from samples (§4.13).

Out of scope for this feature: the invoice PDF and print layout (FR-ORD-10, FR-ORD-15, §6: Q12); adding lines from the class book list (FR-ORD-02, the Planning module); payments and their allocation to invoices (the Payment module); attachments on documents (owner's note, backlog); the Super Admin's screens in the `admin` app.

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

## 4. Data

Schema `sales` (all company-owned, platform P18):
- `school_order` (aggregate root): `number`, `school_id`, `order_date`, `session`, `is_gst_applied`, `total_discount_type` (`NONE`, `PERCENT`, `AMOUNT`), `total_discount_value`, `remark`, `status` (`DRAFT`, `CONFIRMED`, `CLOSED`), `close_reason`, the computed totals (subtotal, total discount, taxable, GST, round-off, grand total), audit and concurrency columns. Unique `(company_id, number)`.
- `school_order_line` (child): `item_id`, `price_version_id`, item code, name, HSN and GST rate, price version name, `mrp_amount`, `rate_amount`, `quantity`, `item_discount_type`, `item_discount_value`, the computed amounts, `invoiced_quantity`.
- `invoice` (aggregate root): `number`, `financial_year`, `invoice_date`, `school_order_id`, school snapshot (id, code, name, state, GSTIN), company state, `session`, `is_gst_applied`, the discounts, every total, `status` (`ISSUED`, `CANCELLED`), `cancel_reason`, `cancelled_at`, audit and concurrency columns. Unique `(company_id, number)`.
- `invoice_line` (child): the order line id, every copied value of R8 and its computed amounts (gross, item discount, line amount, total discount share, taxable, CGST, SGST, IGST).

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session; the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `POST school-orders` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | a new draft `{schoolId, orderDate?, session?, isGstApplied?, lines?, totalDiscount?, remark?}` -> `201` with the number and the computed totals |
| `PUT school-orders/{id}` | same | replaces a draft's values and lines; `409` when not a draft |
| `DELETE school-orders/{id}` | same | a draft only (soft delete); `409` otherwise |
| `POST school-orders/{id}/confirm` | same | R6 |
| `POST school-orders/{id}/close` | `OWNER`, `SUPER_ADMIN` (entered) | `{reason}` R6 |
| `GET school-orders`, `GET school-orders/{id}` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R15; the detail has the lines with remaining quantities, the stock warning of R5 and the order's invoices |
| `POST school-orders/{id}/invoices` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | `{invoiceDate?, lines: [{orderLineId, quantity}]}` -> `201` with the invoice and the items at or below zero (R8-R11) |
| `POST invoices/{id}/cancel` | `OWNER`, `SUPER_ADMIN` (entered) | `{reason}` R12 |
| `GET invoices`, `GET invoices/{id}` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R15 |

Errors are `ProblemDetails` with a message code from the module's catalogue.

## 6. User interface

Written in the UI plans; summary (platform U14, owner's document pattern): the school's profile gets the tabs **Orders** and **Invoices**; "New order" opens the order editor as a full-width page (an order holds 100-200 lines, Q11) with barcode and search entry, the price version per line, discounts, the GST toggle, live totals and the yellow stock warning; a confirmed order shows "Make invoice" (quantities per line, remaining by default). An invoice opens with a header showing its money story (ordered, invoiced, cancelled) and its lines, discounts, GST, round-off and grand total; the Owner cancels it with a reason. Required fields carry a red `*`; each state shows loading, empty, error with retry and success.

## 7. Events

Raised by the aggregates (past tense, ids and values only): `SchoolOrderConfirmed`, `SchoolOrderClosed`, `InvoiceIssued` (invoice id, school, grand total), `InvoiceCancelled`. Reactions: none in this feature (the postings are made in the same use case through the capabilities, R11, R12).

## 8. Dependencies

`sales` depends on `school` (directory R16, session R18), `item` (directory R16), `price` (directory R18), `stock` (posting R11, quantities R20), `finance` (posting R5), `company` (tax profile R16), and the platform (P18-P24). Payment and returns will depend on `sales` by the invoice id.

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

## 10. Decisions

| # | Decision |
|---|---|
| D1 | New module `sales` with its own schema for school orders and invoices (later counter sale and returns). `SchoolOrder` and `Invoice` are separate company-owned aggregates; an invoice refers to its order by id and the order keeps the invoiced quantities. (proposed by the assistant, Q1) |
| D2 | The amounts of R7 are one domain rule used by the order and the invoice, so an order's preview and its invoice can never compute differently. (proposed by the assistant) |
| D3 | Making and cancelling an invoice post stock and ledger through their capabilities in the same use case and transaction (stock R11, finance R5), not through events, because the invoice must fail as a whole when a posting fails. (proposed by the assistant) |

## 11. Open questions

None. Q1-Q14 were answered by the owner one by one on 2026-11-15; the rules above follow the answers. The list keeps the questions with the answer taken.

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
- **Q12** Invoice PDF (FR-ORD-10, FR-ORD-15, §6). Recommended: a later feature for all documents together, after a PDF package is approved; meanwhile the invoice screen. Alternative: now, with this feature.
- **Q13** An item with no price version. Recommended: it cannot be added to an order (R3).
- **Q14** Closing an order early. Recommended: the Owner closes a confirmed order with a reason when the rest will not be delivered (R6).
