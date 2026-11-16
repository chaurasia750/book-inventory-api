# Spec: purchase - Vendor returns (and, later, purchases)

- **Module:** `purchase` (PostgreSQL schema `purchase`)
- **Status:** R1-R13 and AC1-AC10 (the vendor return and its Debit Note PDF) Approved by abhishek in chat on 2026-11-16 (Q1-Q7 answered one by one; D1-D3 with them)
- **Last updated:** 2026-11-16 (new module, vendor return)
- **Source docs:** [BRD §2, §3 (who does what), §4.12 B Vendor Return (FR-RET-10 to FR-RET-14), §5.5 the stock movement table, §5.6 BR-LED, §6 Documents (Debit Note), FR-CMP-04, FR-LED-02, §8 Data Model (PurchaseReturn)](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/platform.md](platform.md) P18-P25; [specs/vendor.md](vendor.md) R15; [specs/item.md](item.md) R16; [specs/price.md](price.md) R19; [specs/stock.md](stock.md) R5, R9, R11; [specs/finance.md](finance.md) R2, R5, R7; [specs/sales.md](sales.md) R7 (the rounding rule), R57-R59 (the credit note this document mirrors)
- **Plans:** [purchase#01](../plans/purchase/2026-11-16-01-vendor-return.md) (Approved, R1-R10, R12, R13, AC1-AC8, AC10), [purchase#02](../plans/purchase/2026-11-16-02-ui-vendor-return.md) (Approved, UI), [purchase#03](../plans/purchase/2026-11-16-03-debit-note-pdf.md) (Approved, R11, AC9), [purchase#04](../plans/purchase/2026-11-16-04-ui-debit-note-pdf.md) (Approved, UI)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`purchase` holds what the company buys from and gives back to its vendors (publications). The module is new; its first feature is the **vendor return**: books or other items sent back to a vendor, which take the stock out (from Main or Damaged stock), lower what the company owes the vendor through the vendor's ledger and are written on a **Debit Note**. The vendor order (BRD §4.8), the goods received, **Maal Aaya** (§4.9), the vendor payment and the sample returns to a vendor are later features of this module and are not part of this one.

Feature **Vendor return (Debit Note)** (R1-R13): described in sections 2 and 3.

Out of scope for this feature: a vendor's refund (a vendor never pays money back, so there is no refund entry, FR-RET-14); GST on the return (Q1); a link to the purchase the books came from (the purchase feature is not built yet); the Super Admin's screens in the `admin` app.

## 2. Concepts and glossary

- **Vendor return**: a document that sends items back to one vendor; it has lines, a number and a date, and is never edited, only cancelled (R2, R8).
- **Debit Note**: the printed form of a vendor return (R11); the ledger **debits** the vendor with it, which lowers the vendor's balance (finance R7).
- **Stock source**: where the returned books come from, **Main** (good) or **Damaged** stock (FR-RET-11, FR-STK-07).
- **Amount-only line**: a line of an item that is not stock managed; it lowers the vendor's balance but moves no stock (FR-RET-12).

## 3. Business rules

**Vendor return (Debit Note)** (BRD §4.12 B, FR-RET-10 to FR-RET-14; Approved by abhishek 2026-11-16, proposed by the assistant from the BRD)
- **R1** Vendor returns belong to exactly one company, the company of the session (platform P18-P21); another company's record is `404`, identical to an unknown id. Vendors, items, prices, stock and the ledger are reached only through their modules' capabilities (vendor R15; item R16; price R19; stock R11; finance R5), never by joining their tables.
- **R2** A **vendor return** is made for one **active** vendor (Q5) and has one or more lines, at most 500, and an optional remark (at most 500 characters). It is complete when made (R10) and never edited; the Owner cancels it (R8).
- **R3** It has a generated **number** per company and financial year, `DN/2026-27/0001`, starting again at 0001 each financial year, from the company code series (platform P23, FR-CMP-04 "Debit Note"), never given twice, also not after a cancel; a cancelled return keeps its number. Its **date** is the business date by default, may be earlier, never in the future; its **financial year** is the April-March year of that date (finance R4).
- **R4** A **line** has: an active **item**, found by name, code or barcode (FR-RET-10, item R16), the **rate**, the **stock source** and the **quantity** (a whole number from 1). The rate starts at the item's **purchase price** in its default price version on the return date (FR-RET-10, price R19, Q3) and the user may change it; a rate has at most two decimals and is above zero. The stock source is `MAIN` or `DAMAGED` (FR-RET-11); a line has no discount (Q4).
- **R5** **Amounts**: a line amount is rate x quantity rounded to 2 decimals half away from zero (the rule of sales R7, written once there); the **total** is the sum of the line amounts, rounded to the nearest rupee, half away from zero, the difference being the round-off (Q2). No GST is applied (Q1). A total that is not above zero is refused.
- **R6** **Low stock never blocks** (stock R9): a return of more than the stock holds is accepted and the answer lists the items whose stock went to zero or below.
- **R7** **Making a vendor return** posts, in the same request transaction: the **stock out** of every stock-managed line (stock R11: movement kind `VENDOR_RETURN`, from Main or Damaged as the line says; items that are not stock managed are skipped, FR-RET-12) and one **ledger transaction** (finance R5, BR-LED-02): **debit** `VENDOR_PAYABLE` (the vendor) the total, **credit** `PURCHASE_RETURN` the sum of the line amounts, and the round-off on `ROUND_OFF`; the document is the vendor return and its date the return date. The vendor ledger shows it as `Debit Note (Dr)` (FR-LED-02) and the vendor's balance falls by the total. There is **no refund entry**: a vendor does not pay money back (FR-RET-14).
- **R8** The Owner **cancels** a vendor return with a reason (5-500 characters): the stock and ledger postings are reversed dated the business date of the cancel (stock R11, finance R5), it becomes `CANCELLED` and keeps its number; the vendor's balance and the stock are as before it was made.
- **R9** **Who does what** (BRD §3: "Return" is for the Owner and Staff, cancelling a return for the Owner only, Q6): the Owner, Staff and an entered Super Admin make vendor returns and read them; only the Owner (and an entered Super Admin) cancels one. Anonymous `401`.
- **R10** **Snapshot** (BR-PRC-03): a vendor return keeps the vendor's name and code, the company's printed identity (name, address, GSTIN, state, contact, footer) and every line's item code, name, rate and quantity as they were when it was made; a later change of an item, a price or the vendor changes no vendor return.
- **R11** The **Debit Note PDF** (FR-RET-13, BRD §6 "Debit Note, Vendor return, amount, A4"): `GET vendor-returns/{id}/pdf`, an **A4** PDF titled `DEBIT NOTE`, `inline`, named `<number with / as ->.pdf`, rendered on demand and never stored, made like the credit note PDF (sales R71): the company's identity as kept (R10) with the live logo and signature, the number, the date, the vendor, each line (item, stock source, rate, quantity, amount), the total and its round-off, the total in words, the remark and the footer line; a cancelled one prints `CANCELLED` with its reason; `Page x of y` and the header on every page. Another company's record is `404`.
- **R12** **Reading**: the company's vendor returns, paged (page size capped at 200), searchable by number and vendor name or code, filterable by vendor, status and date range, newest first; one shows its lines, total and remark. The vendor's page lists its vendor returns (sales R15's rule).
- **R13** Every create and cancel is recorded who and when (the standard audit columns and the audit trail of `core`); money is `numeric(18,2)`, never float; every input is checked at the API boundary (lengths, ranges, at most 500 lines, whole quantities, two decimals) and the rules again in the domain; a vendor return posts once and a retry never counts twice (finance R5, R15).

## 4. Data

Schema `purchase` (all company-owned, platform P18):
- `vendor_return` (aggregate root): `number`, `financial_year`, `return_date`, `vendor_id`, vendor snapshot (code, name), the company snapshot of R10 (name, address, mobile, email, GSTIN, PAN, tagline, state, terms, footer), `remark`, `status` (`ACTIVE`, `CANCELLED`), `cancel_reason`, the amounts (`subtotal_amount`, `round_off_amount`, `grand_total_amount`), audit and concurrency columns. Unique `(company_id, number)`; index `(company_id, vendor_id)`.
- `vendor_return_line` (child): `item_id`, item code and name, `stock_source` (`MAIN`, `DAMAGED`), `rate_amount`, `quantity`, `line_amount`.

## 5. API contract (`/api/v1`)

All routes are company-scoped by the session; the company is never a request field.

| Method + route | Who | Notes |
|---|---|---|
| `POST vendor-returns` | `OWNER`, `STAFF`, `SUPER_ADMIN` (entered) | R2-R7: `{vendorId, returnDate?, remark?, lines:[{itemId, quantity, stockSource, rate?}]}` -> `201` with the number, the total and the items whose stock went to zero or below |
| `GET vendor-returns`, `GET vendor-returns/{id}` | same | R12: paged list with the filters; the detail with its lines |
| `POST vendor-returns/{id}/cancel` | `OWNER`, `SUPER_ADMIN` (entered) | R8: `{reason}` -> `204`; `409` when already cancelled |
| `GET vendor-returns/{id}/pdf` | same as reading | R11: the A4 Debit Note, `inline` |

Errors are `ProblemDetails` with a message code from the module's catalogue.

## 6. User interface

A **Purchases** menu (Owner and Staff) with **Vendor returns**: the list beside a panel (platform U10), the form for a new return (vendor, date, items found like the order editor, the stock source and the quantity of each line, the rate shown and changeable, the remark), the panel with the lines, the total and, for the Owner, Cancel. Print and Download PDF in the panel give the Debit Note (R11). Written in the UI plans.

## 7. Events

Raised by the aggregate (past tense, ids and values only): `VendorReturnMade` (id, vendor, number, total), `VendorReturnCancelled`. Reactions: none in this feature.

## 8. Dependencies

`purchase` depends on `vendor` (directory R15), `item` (directory R16), `price` (the purchase-price capability, R19), `stock` (posting R11, quantities R20), `finance` (posting R5), `company` (the printed identity, R17-R20) and the platform (P18-P25). None of them depends on `purchase`.

## 9. Acceptance criteria

- **AC1** The Owner returns 5 copies of an item (purchase price 180) from Main stock to a vendor: `201`, number `DN/2026-27/0001`, total 900.00; Main stock falls by 5; the vendor's balance (finance R7) falls by 900.00; the ledger transaction is debit `VENDOR_PAYABLE` 900.00, credit `PURCHASE_RETURN` 900.00 (R3-R7).
- **AC2** The same return from `DAMAGED` stock takes Damaged stock down by 2 and leaves Main alone; the vendor's balance falls the same way (R4, R7).
- **AC3** A line whose rate is changed to 175.50 for 3 copies gives a line amount 526.50 and a total of 527 with round-off +0.50 on `ROUND_OFF` (R4, R5, R7).
- **AC4** A line of an item that is not stock managed lowers the vendor's balance and moves no stock (R7).
- **AC5** Returning more than the stock holds answers `201` with the item in the zero-or-below list (R6).
- **AC6** A return for an inactive or unknown vendor is `400`/`404` and records nothing; a date tomorrow, no lines, a rate of 0 or a quantity of 0 is `400` (R2-R5).
- **AC7** The Owner cancels a vendor return with the reason `entered twice`: `204`, stock and the vendor's balance are as before, the number stays; Staff get `403`; cancelling twice is `409` (R8, R9).
- **AC8** Two returns on the same day get `DN/2026-27/0001` and `0002`; company B's first is `0001` again; company A reading company B's return gets `404` (R1, R3).
- **AC9** `GET vendor-returns/{id}/pdf` answers `200` an A4 PDF whose text has the company name, `DEBIT NOTE`, the number, the date, the vendor, every line and the total in words; a cancelled one shows `CANCELLED` (R11).
- **AC10** The list filters by vendor, status and date range; the detail shows its lines (R12).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | Proposed by the assistant 2026-11-16 and chosen by abhishek in chat 2026-11-16: **a new module `purchase`** holds the vendor return, because the BRD's PurchaseReturn belongs with Maal Aaya, the vendor order and the vendor payment (later features), and putting it in `vendor` would make that module depend on stock and finance, which already read it. |
| D2 | Proposed by the assistant 2026-11-16: **there is no refund entry** (FR-RET-14); the vendor ledger is only debited. |
| D3 | Proposed by the assistant 2026-11-16: **the vendor return mirrors the credit note** of sales R53-R63 in numbering, snapshot, cancel and PDF, with `VENDOR_PAYABLE`/`PURCHASE_RETURN` in place of `SCHOOL_RECEIVABLE`/`SALES_RETURN`. |

## 11. Open questions

- **Q1** GST on a vendor return? The BRD says nothing (FR-PUR-06 makes GST on a purchase bill optional and the purchase is not built). Recommended: **none in this feature** (amount only; `GST_INPUT` stays unused until the purchase feature decides), alternative: an optional GST rate per line posted to `GST_INPUT`. Answered by abhishek 2026-11-16: **none, amount only**.
- **Q2** Rounding of the total? Recommended: **to the nearest rupee, half away from zero, the difference on `ROUND_OFF`**, the same as every sales document (sales R7); alternative: exact paise. Answered by abhishek 2026-11-16: **to the nearest rupee, half away from zero, the difference on ROUND_OFF**.
- **Q3** Where does the starting rate come from? Recommended: **the item's purchase price in its default version on the return date, through a new price capability (price R19)**, always changeable; alternative: the rate is always typed by hand (no price capability). Answered by abhishek 2026-11-16: **the item's purchase price in its default version on the return date, through the new price capability (price R19), always changeable**.
- **Q4** A discount on a returned line? Recommended: **none** (the rate is changed instead); alternative: item and total discount like a purchase bill. Answered by abhishek 2026-11-16: **no discount**.
- **Q5** A return to an **inactive** vendor? Recommended: **refused** (like orders for an inactive school), the Owner reactivates the vendor first; alternative: allowed. Answered by abhishek 2026-11-16: **an inactive vendor is refused**.
- **Q6** Who makes a vendor return? Recommended: **the Owner and Staff make, only the Owner cancels** (BRD §3 puts "Return" for both and "cancel a return" for the Owner); alternative: the Owner only, because the vendor screens are the Owner's today. Answered by abhishek 2026-11-16: **the Owner and Staff make, only the Owner cancels**.
- **Q7** The Debit Note PDF: **A4 like the credit note, with `Page x of y` and a repeated header**, no bank details (BRD §6)? Recommended: yes; alternative: one page. Answered by abhishek 2026-11-16: **A4 like the credit note, Page x of y, no bank details**.
