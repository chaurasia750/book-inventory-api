# Plan sales#11: Dynamic terms and conditions (placeholders)

- **File:** `docs/plans/sales/2026-11-15-11-dynamic-terms.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R35; AC27
- **Owner:** abhishek
- **Status:** Done (2026-11-15)
- **Base branch:** `feature/company-11` (branch `feature/sales-11`)
- **Depends on:** `sales#07` (Done)
- **UI plan:** none beyond a hint under the terms box of the Company profile screen (company#08 lists the placeholders)
- **Approved by / on:** abhishek / 2026-11-15 (chat: "invoice terms fix rahega, dynamic value hoga, each invoice nhi": one fixed text per company with placeholders)
- **Created:** 2026-11-15

## 1. In one minute
After this plan the company's terms text can hold placeholders such as `{{school_name}}` or `{{invoice_number}}`, and every invoice PDF fills them with that invoice's own values. Try it: write `Supplied to {{school_name}} under {{invoice_number}}` as the terms, make an invoice and open its PDF.

## 2. Touches / leaves out
Touches: `Application/Sales` (one `InvoiceTerms` that fills the placeholders; `InvoicePdfModelBuilder` calls it before it numbers the terms), `docs/Code_Map.md` only if a new pattern appears.
Leaves out: a terms text per school or per invoice (not wanted); a due date placeholder (needs credit days); the screens. No schema change: the terms text is already kept on the invoice.

## 3. Approach
The invoice keeps the company's terms text as typed (sales R13, R33). When the PDF is printed, `{{name}}` tokens are replaced from the invoice's own values (company name, school name and code, invoice number and date, financial year, session, order number); a token not in the list stays as typed. The filled text is HTML-encoded and numbered line by line like before.

## 4. Tasks (at most 8)
- [x] 1. `InvoiceTerms.Resolve(text, values)` for `{{name}}` tokens (spaces allowed, unknown left as typed) (R35).
- [x] 2. The PDF model builder fills the terms from the invoice's values before numbering them (R35).
- [x] 3. Build at 0 warnings; verify on the real local database with a PDF.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R35`, `AC27` | 1, 2, 3 | **PASS** (6 of 6, scratch `check_terms.py`, real `book_inventory`, PDFs read per page). Terms `Goods supplied to {{school_name}} ({{school_code}}) under {{ invoice_number }} dated {{invoice_date}}.` + `Disputes: {{unknown_thing}}`: the PDF printed `Goods supplied to Terms School One 759e1 (SCH-0092) under INV/2026-27/0083 dated 15 Nov 2026.` and `{{unknown_thing}}` as typed; the second invoice (school name `Terms <&> School Two`) printed its own name, code and number, the `<&>` as text; after the company changed its terms, the reprint of the first invoice still had the old text; terms without placeholders printed unchanged. The ABC terms were put back and the test invoices cancelled. |

## 6. Not verified
The hint listing the placeholders under the terms box is company#08 (screen, not seen in a browser). A placeholder written with capital letters (`{{School_Name}}`) is not filled (names are lower case) and prints as typed; not checked.

## 7. Risks and notes
No migration. The terms of an invoice made before the placeholders existed have none and print unchanged.
