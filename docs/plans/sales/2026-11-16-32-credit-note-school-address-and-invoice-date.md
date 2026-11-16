# Plan sales#32: Credit note PDF - the school's address and the invoice's date

- **File:** `docs/plans/sales/2026-11-16-32-credit-note-school-address-and-invoice-date.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R72; AC58
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#28` (Done, the credit note PDF)
- **UI plan:** none, the credit note download is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "next item kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan a new credit note prints the school's address and the date of the invoice it refers to (GST Rule 53). See it: make a credit note against an invoice and download its PDF.

## 2. Touches / leaves out
Touches: the `CreditNote` aggregate (two values kept at creation), its EF configuration, one migration (`sales.credit_note.school_address`, `invoice_date`), `CreditNotePdfModelBuilder` and `credit-note-body.sbn`.
Leaves out: the debit note (Backlog B148: needs the owner's decision on purchase GST), a back-fill of older credit notes, the school code on the credit note, the screens.

## 3. Approach
Like the invoice (which keeps `SchoolAddress`), the credit note keeps what it printed at creation, so a later change of the school's address does not change an old credit note. Schema change: **yes**, two nullable columns, additive; the only open schema-changing plan of the `sales` module. Rollback: `Down` drops the two columns.

## 4. Tasks (at most 8)
- [x] 1. `CreditNote`: `SchoolAddress` and `InvoiceDate`, filled in `Create` from the school and the returned invoice.
- [x] 2. EF configuration (address up to 500 characters) and the migration `Sales_CreditNoteSchoolAddressAndInvoiceDate`; its SQL read, applied to the local development database.
- [x] 3. Model builder and template: the address under the school's name, `dated <date>` after the invoice number.
- [x] 4. Build at 0 warnings, 0 errors.
- [x] 5. Verify the row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R72`, `AC58` | 1-3 | a school with an address; a credit note against its invoice; the PDF has the address and `Invoice INV/... dated ...`; an older credit note prints without them |

## 6. Not verified
Observed (2026-11-16, target database `book_inventory` on `localhost`, the local development database): the migration `20261116141455_Sales_CreditNoteSchoolAddressAndInvoiceDate` was applied by the start of the scratch API (two `ADD COLUMN` of nullable `date` and `varchar(500)`, no data touched). The school SCH-0097 was given a fake address `Test Road, Test City`; a credit note `CN/2026-27/0010` against invoice `INV/2026-27/0058` was made: its PDF has `Test Road, Test City` and `Invoice INV/2026-27/0058 dated 16 Nov 2026`; the older credit note `CN/2026-27/0009` prints without an address and without `dated`; the school's address was put back afterwards. Build 0 warnings, 0 errors.
NOT verified: that a later change of the school's address leaves the printed credit note as it was (read from the code: the value is copied at creation), a return without an invoice, and the page image. The test credit note `CN/2026-27/0010` stays in the local database (Backlog B27 test data).

## 7. Risks and notes
The migration was applied to the local database only; shared, staging and production databases are the owner's to apply (the application migrates on start unless `Database__MigrateOnStartup=false`).
