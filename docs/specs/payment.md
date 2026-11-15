# Spec: payment - School payment receipts

- **Module:** `payment` (PostgreSQL schema `payment`)
- **Status:** R1-R20 and AC1-AC15 Approved by abhishek on 2026-11-15 ("yes all approve hai"; Q1-Q7 answered one by one, all with the recommended answers)
- **Last updated:** 2026-11-16 (payment#10 done: Print and Download PDF on the receipt; every plan of the first payment feature is done)
- **Source docs:** [BRD §4.14 Payment (A. School se Payment Receive, FR-PAY-01 to FR-PAY-08), §5.6 BR-LED-01 and BR-LED-05, §6 Documents (Payment Receipt, A5), §8 Data Model (PaymentReceipt, PaymentAllocation), §9 Q-08](../bussiness/BRD-Book-Inventory-Phase1.md); [specs/finance.md](finance.md) (R2 accounts, R5 posting, R7 balance); [specs/sales.md](sales.md) (invoices); [specs/company.md](company.md) (D5, R19, R20)
- **Plans:** [payment#01](../plans/payment/2026-11-15-01-receipt-core.md) (Done 2026-11-16, R1-R9, R16 list and detail, R18, R19, AC1-AC7, AC13), [payment#02](../plans/payment/2026-11-15-02-ui-receipt-core.md) (Done 2026-11-16, UI), [payment#03](../plans/payment/2026-11-15-03-cheque-status.md) (Done 2026-11-16, R10-R12, AC8-AC10), [payment#04](../plans/payment/2026-11-15-04-ui-cheque-status.md) (Done 2026-11-16, UI), [payment#05](../plans/payment/2026-11-15-05-advance-and-cancel.md) (Done 2026-11-16, R13-R15, AC11, AC12), [payment#06](../plans/payment/2026-11-15-06-ui-advance-and-cancel.md) (Done 2026-11-16, UI), [payment#07](../plans/payment/2026-11-15-07-payment-proof.md) (Done 2026-11-16, R20, AC15), [payment#08](../plans/payment/2026-11-15-08-ui-payment-proof.md) (Done 2026-11-16, UI), [payment#09](../plans/payment/2026-11-15-09-receipt-pdf.md) (Done 2026-11-16, R17, AC14), [payment#10](../plans/payment/2026-11-15-10-ui-receipt-pdf.md) (Done 2026-11-16, UI); depends on [sales#14](../plans/sales/2026-11-15-14-invoice-directory.md) (Approved)

> One spec per module. It says WHAT the module does and WHY, never implementation steps (those live in plans).

## 1. Purpose and scope

`payment` takes the money a **school** pays: one **receipt** per payment, with one or more **modes** (cash, UPI, bank transfer, cheque), applied to the school's invoices (oldest first or hand-picked) with the rest kept as **advance**. Every receipt credits the school's ledger (finance R5), a cheque has a status (pending, cleared, bounced), and a receipt can be printed as an A5 PDF with the company's logo.

Out of scope for this feature: the payment to a **vendor** (FR-PAY-09 to FR-PAY-12) and the payment from a **shop customer** (FR-PAY-13 to FR-PAY-15), which are later features of this module (the receipt and mode concepts are written so they can be reused); refunds to a school and the return credit note (a later `sales` feature, BR-LED-01); the ledger itself (`finance`) and the invoices (`sales`), which this module reads and posts to through their capabilities.

## 2. Concepts and glossary

- **Receipt**: the document of one school's payment on one date: a number, the school, the date, the modes, the allocations and a status (`RECORDED` or `CANCELLED`).
- **Mode**: one way the money came: `CASH`, `UPI`, `BANK_TRANSFER` or `CHEQUE`, with its amount and its reference (BRD §4.14 A).
- **Allocation**: the part of a receipt applied to one invoice. The **allocated** amount of an invoice is the sum of its allocations; its **due** is its grand total minus that.
- **Advance**: the part of a receipt applied to no invoice (receipt total, minus allocations, minus bounced cheques).
- **Open invoice**: an invoice that is `ISSUED` (not cancelled) and has a due above zero.
- **Cheque status**: `PENDING` when received, then `CLEARED` or `BOUNCED`, once.

## 3. Business rules

**School payment receipt** (FR-PAY-01 to FR-PAY-04, FR-PAY-06, FR-PAY-08)
- **R1** Everything belongs to one company, the company of the session (platform P18-P21). The school is read through its directory (school R16), the invoices through the sales invoice directory (sales R37), the balance and the postings through the finance capabilities (finance R5, R7), never by joining their tables.
- **R2** **Who** (company D5: Staff only create orders and sell): the Owner and a Super Admin who entered the company record receipts, adjust advances, set cheque results, cancel receipts and read; Staff and anonymous callers get `403` and `401`.
- **R3** **Dues first** (FR-PAY-01): for a school the screen shows its **balance** (finance R7) and its **open invoices**, oldest first (invoice date, then number): number, date, grand total, allocated, due. A school with an advance shows a negative balance (finance R7).
- **R4** A receipt has: the **school** (existing; an inactive school may still pay, Q5), the **receipt date** (default today in the business time zone; not after today; an earlier date is allowed, Q6), **one to ten modes** (R5) and an optional remark (at most 500 characters). Its **total** is the sum of its modes and must be above zero.
- **R5** **Modes** (FR-PAY-02, BRD §4.14 A): each mode has an **amount** above zero (at most two decimals, at most 9,999,999,999.99) and: `CASH` nothing more; `UPI` the transaction id (4-50 characters); `BANK_TRANSFER` the UTR number (4-50 characters); `CHEQUE` the cheque number (4-20 characters), the cheque date and the school's bank name (at most 100 characters). A mix is one receipt (₹5,000 cash + ₹10,000 cheque). A missing or wrong field is `400` with its code.
- **R6** **Allocation** (FR-PAY-03, FR-PAY-04): by default the total is applied **oldest invoice first** across the school's open invoices, each up to its due, until the total is used; what is left is **advance**. The caller may instead give the allocations by hand (invoice, amount): each invoice must be an open invoice of the same school, each amount above zero and at most that invoice's due, and the amounts must add up to at most the total; the rest is advance. An invoice of another school or a cancelled one is `400`.
- **R7** **Ledger** (FR-PAY-06, BR-LED-01, finance R5): recording a receipt posts, in the same request transaction, one ledger transaction of document type `PAYMENT_RECEIPT`: credit `SCHOOL_RECEIVABLE` (the school) the total; debit `CASH` the cash modes, `BANK` the UPI and bank-transfer modes and `CHEQUE_IN_HAND` the cheque modes (finance R2); the date is the receipt date. The payment is therefore received **on the school's ledger** (owner, 2026-11-15: payment comes to the school and is then adjusted to invoices); the allocation of R6 only matches that credit to invoices and the ledger never needs it, so adjusting an advance later or cancelling an invoice changes the matching, not the school's balance. The narration names the receipt number and its modes, so the school's statement (finance R9) shows each payment. The company's single bank account is printed on the receipt, never chosen (FR-PAY-08, company R19).
- **R8** The receipt **number** is per company and per financial year of its date: `RCP/2026-27/0001`, starting again at 0001 each financial year, from the company code series (platform P23). A number is never given twice, also not after a cancel.
- **R9** **Idempotency** (CLAUDE.md "Money"): the caller sends a `clientRequestId` (a UUID) with every receipt; the same id from the same company returns the receipt already recorded (`200`) instead of recording a second one, so a double click or a retry never counts twice.

**Cheque status** (FR-PAY-07)
- **R10** Each `CHEQUE` mode starts `PENDING`. The Owner sets it once to **cleared** (with the clearing date, not before the cheque date) or **bounced** (with the bounce date and a reason of 5-500 characters); a result is final (`409` when set again). **Cleared** posts a transaction `CHEQUE_CLEARED` (document id = the mode's id): debit `BANK`, credit `CHEQUE_IN_HAND`, the cheque amount, dated the clearing date. **Bounced** posts `CHEQUE_BOUNCED`: debit `SCHOOL_RECEIVABLE` (the school), credit `CHEQUE_IN_HAND`, the cheque amount, dated the bounce date, with the reason as narration, so the school's balance rises again (BR-LED-01).
- **R11** **A bounce and the allocations** (Q3): the bounced amount is taken first from the receipt's **advance**, then from its allocations starting with the **most recently allocated** invoice, until it is covered; those invoices' dues rise by the amounts taken. Nothing else of the receipt changes.
- **R12** **An invoice that is cancelled** (sales R12; Q4): what was allocated to it becomes **advance** of the receipts it came from; the receipts stay as they are. The module reacts to the sales event of the cancel in the same transaction, so no payment is lost.

**Advance and cancel** (FR-PAY-03, BR-LED-05)
- **R13** **Adjusting an advance** (Q2): the Owner applies a receipt's advance to open invoices of the same school by the rules of R6 (oldest first, or by hand); it posts nothing to the ledger, the money is already in the school's credit.
- **R14** **Cancelling a receipt** (BR-LED-05: nothing posted is deleted): the Owner cancels a `RECORDED` receipt with a reason (5-500 characters) while **none of its cheques has a result**; it posts the exact opposite of the receipt's transaction (finance R5) dated the business date of the cancel, removes all its allocations (the invoices' dues return) and the receipt becomes `CANCELLED`, keeping its number. A receipt with a cleared or bounced cheque answers `409 payment.receipt_has_cheque_result`. A cancelled receipt has no advance (its advance is 0 in every answer).
- **R15** A receipt is **never edited or deleted**: a mistake is fixed by cancelling it and recording it again.

**Reading and printing** (FR-PAY-05)
- **R16** **Reading**: the receipts of the company paged (page size capped at 200), searchable by number and school name or code, filterable by school, status, date range, mode and cheque status (the list of pending cheques is the filter `PENDING`), newest first; a receipt's detail has its modes, allocations (invoice number, amount), advance and status. A school's profile shows its receipts (plan UI). The **dues of invoices** (allocated and due for up to 100 invoice ids) are readable for the invoice screens.
- **R17** **Receipt PDF** (FR-PAY-05, BRD §6): an **A5** PDF made like the invoice PDF (sales R28 to R33): the company's logo, name and contact, the receipt number and date, the school, each mode with its reference, the total in words (Indian numbering), the invoices it was applied to and the advance, the company's one bank account and signature; a cancelled receipt is marked CANCELLED.
- **R18** Every create and change records who and when (platform audit columns); the ledger transactions keep their own history (finance R14).
- **R20** **Proof of payment** (owner, 2026-11-15, optional): the Owner may attach proof to a receipt, such as a photo of the cheque, a UPI screenshot or a bank slip: **up to 5 files** per receipt, each a PNG, JPEG or PDF of **at most 5 MB** (anything else is `400 payment.proof_invalid`), added while the receipt is `RECORDED`, viewable and removable by the Owner (a removed proof is soft-deleted), kept in the database with the receipt like the company's logo (company Q18). A receipt needs no proof; a proof is not printed on the receipt PDF (R17).
- **R19** The **financial year** of a receipt is April to March of its date (finance R4); the receipt takes no session (finance R3: optional).

## 4. Data

Schema `payment` (every table company-owned, platform P18):
- `receipt` (aggregate root): `id` (UUID v7), `number`, `financial_year`, `school_id`, `receipt_date`, `total_amount` (`numeric(18,2)`), `status` (`RECORDED`/`CANCELLED`), `cancel_reason`, `cancelled_on` (the business date of the cancel), `remark`, `client_request_id` (unique with the company), plus the standard audit and concurrency columns.
- `receipt_mode` (child): `mode_type` (`CASH`/`UPI`/`BANK_TRANSFER`/`CHEQUE`), `amount` (`numeric(18,2)`), `reference` (UPI id or UTR), `cheque_number`, `cheque_date`, `bank_name`, `cheque_status` (`PENDING`/`CLEARED`/`BOUNCED`, only for a cheque), `result_date`, `bounce_reason`.
- `receipt_proof` (child, R20): `file_name`, `content_type`, `content` (`bytea`), `size_bytes` (listed oldest first by id; a child row keeps no upload time); soft-deleted when removed.
- `receipt_allocation` (child, **insert-only**): `invoice_id` (plain id, no foreign key across schemas), `amount` (`numeric(18,2)`, **signed**: a reduction by a bounce, a cancelled invoice or a cancelled receipt is a new negative row, never an edit), `reason` (`ALLOCATED`, `BOUNCED`, `INVOICE_CANCELLED`, `RECEIPT_CANCELLED`, `ADJUSTED`), `sequence` (the row's place in the receipt's history, which decides the most recently allocated invoice, R11); the standard audit columns hold who and when (there is no separate `allocated_at`). The allocated amount of an invoice is the sum of its rows.

Money is exact decimal with two decimals; no rounding happens here (the amounts arrive in rupees and paise, finance R8).

## 5. API contract (`/api/v1`)

| Method + route | Who | Notes |
|---|---|---|
| `GET payment/dues?schoolId=` | `OWNER`, `SUPER_ADMIN` (entered) | the school's balance and open invoices (R3) |
| `POST receipts` | same | `{clientRequestId, schoolId, receiptDate?, modes: [{type, amount, reference?, chequeNumber?, chequeDate?, bankName?}], allocations?: [{invoiceId, amount}], remark?}` -> `201` with the receipt (`200` for a repeated `clientRequestId`); `400` with codes (R4-R6) |
| `GET receipts`, `GET receipts/{id}` | same | R16 |
| `POST receipts/{id}/cheques/{modeId}/clear` | same | `{date}` (R10); `409` when it has a result |
| `POST receipts/{id}/cheques/{modeId}/bounce` | same | `{date, reason}` (R10, R11) |
| `POST receipts/{id}/adjust-advance` | same | `{allocations?}` (R13) |
| `POST receipts/{id}/cancel` | same | `{reason}` (R14) |
| `POST receipts/{id}/proofs` | same | multipart file (R20); `201` with `{id, fileName, contentType, size}`; `400 payment.proof_invalid` or too many (5) |
| `GET receipts/{id}/proofs/{proofId}`, `DELETE receipts/{id}/proofs/{proofId}` | same | the file with its content type; removes it (R20) |
| `GET receipts/{id}/pdf` | same | R17: `application/pdf`, A5 |
| `GET payment/invoice-dues?invoiceIds=` | same | allocated and due of up to 100 invoices (R16) |

All routes are company-scoped by the session; the company is never a request field. Errors are `ProblemDetails` with a message code from the module's catalogue.

## 6. User interface

Written in the UI plans; summary: the school's profile gets a **Payments** tab (receipts of that school) and a **Receive payment** button (platform U14: details open beside the list); the form shows the school's balance and open invoices first (R3), takes the modes (add a row per mode), shows how the total will be applied (oldest first, editable by hand) and the advance; the receipt screen has the cheque actions, adjust advance, cancel, the optional proof files (attach in the form and on the receipt) and the PDF. Invoice screens show allocated and due. Staff see none of it.

## 7. Events

Raised by the `Receipt` aggregate (past tense, ids and values only): `ReceiptRecorded`, `ChequeCleared`, `ChequeBounced`, `AdvanceAdjusted`, `ReceiptCancelled`. Reacts to: sales `InvoiceCancelled` (R12). No other reaction in this feature.

## 8. Dependencies

`payment` depends on `iam` (current company), the platform (P18-P23), `school` (directory R16), `sales` (invoice directory R37 and the `InvoiceCancelled` event), `finance` (posting R5, balance R7) and `company` (the printed identity, R20, for the PDF). `sales` and `finance` depend on nothing in `payment`: the invoice screens ask `payment` for the dues, so no cycle.

## 9. Acceptance criteria

- **AC1** `GET payment/dues` for a school with invoices of ₹1,000 (older) and ₹500 and nothing paid returns its balance 1,500.00 and the two open invoices oldest first, each due equal to its total (R3).
- **AC2** A receipt of ₹1,200 (cash 200 + UPI 1,000, reference `UPI12345`) with no allocations is applied oldest first: the ₹1,000 invoice is paid in full and ₹200 goes to the ₹500 invoice (due 300); the school's balance is 300.00; the ledger has one `PAYMENT_RECEIPT` transaction: credit school 1,200, debit `CASH` 200, debit `BANK` 1,000 (R6, R7).
- **AC3** A receipt of ₹2,000 against the same dues applies 1,500 and leaves **500 advance**; the balance is -500.00 (R6).
- **AC4** A hand-picked allocation of ₹300 to the ₹500 invoice only is accepted and the rest is advance; ₹600 to it, an invoice of another school, a cancelled invoice and allocations above the total are each `400` (R6).
- **AC5** The same `clientRequestId` sent twice returns `201` then `200` with the same receipt and one ledger transaction (R9).
- **AC6** A cheque mode without its number, date or bank, a UPI mode without its id, a total of zero, an amount with three decimals, eleven modes and a date in the future are each `400` (R4, R5).
- **AC7** Receipt numbers run `RCP/2026-27/0001`, `0002`, ... for a date in the financial year 2026-27 and restart for 2027-28 (R8).
- **AC8** A cheque of ₹10,000 in a receipt that was applied to two invoices: **clear** posts debit `BANK` / credit `CHEQUE_IN_HAND` 10,000 and sets `CLEARED`; setting it again is `409` (R10).
- **AC9** **Bounce** posts debit school / credit `CHEQUE_IN_HAND` 10,000 with the reason and the school's balance rises by 10,000; with 3,000 advance on the receipt, the advance goes to 0 and 7,000 is taken from the most recently allocated invoice back, so its due rises by 7,000 (R10, R11).
- **AC10** Cancelling an invoice that has ₹800 allocated turns the 800 into advance of its receipt; the receipt and its postings are unchanged (R12).
- **AC11** Adjusting an advance of 500 onto an open invoice lowers its due by 500 and posts nothing (R13).
- **AC12** Cancelling a recorded receipt posts the opposite transaction, removes its allocations (the dues return) and marks it `CANCELLED` with its number kept; a receipt with a cleared or bounced cheque answers `409 payment.receipt_has_cheque_result` (R14).
- **AC13** A Staff caller gets `403` on every route; the Owner of company B cannot read or cancel company A's receipt (`404`) (R1, R2).
- **AC15** The Owner attaches a PNG proof to a receipt: `201`, `GET receipts/{id}/proofs/{proofId}` returns the bytes with `image/png`; a `.txt` file, a file above 5 MB and a sixth file are each `400 payment.proof_invalid`; removing it makes the read `404`; a receipt with no proof works as before; Staff get `403`; a cancelled receipt takes no new proof (`409 payment.receipt_not_recorded`) (R20).
- **AC14** `GET receipts/{id}/pdf` answers an A5 PDF whose text has the company name, the receipt number, the school, each mode with its reference, the total in words and the invoice numbers; a cancelled receipt's PDF says CANCELLED (R17).

## 10. Decisions

| # | Decision |
|---|---|
| D1 | Owner, 2026-11-15: a new module `payment` (schema `payment`) holds the school payment receipt now and the vendor and customer payments later; it reads sales and posts to finance, so no module depends back on it (Q1). The module list in `CLAUDE.project.md` gets it in its own small change. |
| D2 | A receipt is a document that is cancelled, never edited or deleted (BR-LED-05); an allocation is an insert-only signed row. (proposed by the assistant) |
| D3 | Account choice (proposed by the assistant, from finance R2, the BRD is silent): cash to `CASH`, UPI and bank transfer to `BANK`, a cheque to `CHEQUE_IN_HAND` until it clears. |
| D4 | The cheque is credited to the school's ledger when received and reversed when it bounces (FR-PAY-06, FR-PAY-07), not when it clears. |
| D5 | Owner, 2026-11-15: an advance stays on its receipt and is adjusted later (R13); a bounce takes the advance first, then the newest allocation (R11); a cancelled invoice turns its payments into advance (R12). (Q2-Q4) |
| D7 | Owner, 2026-11-15: a proof of payment is optional (R20); its limits (5 files, 5 MB, PNG/JPEG/PDF) and its storage in the database are proposed by the assistant, like the company images. |
| D6 | A receipt whose cheque has a result cannot be cancelled (R14): a result already moved money on its own transactions. (proposed by the assistant) |

## 11. Open questions

Answered by abhishek on 2026-11-15, all with the recommended answers:
- **Q1** Where does the payment live? Answered: a **new `payment` module** (D1); alternatives not chosen: inside `sales`, inside `finance`.
- **Q2** What happens to an advance? Answered: it **stays on the receipt and is adjusted to invoices later** (R13); alternatives: visible only, or adjusted automatically on the next invoice.
- **Q3** What does a bounced cheque do to the allocations? Answered: **advance first, then the newest allocation** (R11); alternative: remove all allocations of the receipt.
- **Q4** What if an invoice with payments is cancelled? Answered: **its payments become advance** (R12); alternative: refuse the cancel until the receipt is cancelled.

- **Q5** May a deactivated school still pay its dues? Answered: **yes** (R4); alternative not chosen: no.
- **Q6** May a receipt be dated in the past? Answered: **yes, never in the future** (R4); alternatives not chosen: today only, or within a number of days.
- **Q7** Who may take a school's payment? Answered: **only the Owner** (and an entered Super Admin), as company D5 says Staff only create orders and sell (R2); alternative not chosen: Staff may record a receipt and only the Owner sets cheque results, adjusts and cancels.
