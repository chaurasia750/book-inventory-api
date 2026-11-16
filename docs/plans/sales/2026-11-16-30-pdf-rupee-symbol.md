# Plan sales#30: Rupee symbol in the invoice, credit note, counter bill and refund slip PDFs

- **File:** `docs/plans/sales/2026-11-16-30-pdf-rupee-symbol.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P26, PAC25 (sales part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** none (the shared amount formatter is made here; payment#12, finance#12 and purchase#06 depend on this plan)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "jo plan hai wo approved hai impmemnent kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the PDFs of an invoice, a credit note, a counter bill and a refund slip follow one money rule: `(₹)` in every money column heading, `₹ 1,200.00` on every total and summary amount. See it: download an invoice PDF and look at the Rate, Amount and Grand total.

## 2. Touches / leaves out
Touches: `Application/Sales` (`InvoiceFormat` gets one amount formatter that writes `₹ 1,200.00` and `-₹ 1,200.00`; the four PDF model builders use it for standing amounts and stop typing `₹ ` themselves) and the templates `invoice-body.sbn`, `credit-note-body.sbn`, `counter-bill-body.sbn`, `refund-slip-body.sbn` (column headings).
Leaves out: the other modules' PDFs (payment#12, finance#12, purchase#06), the screens (platform#12), the text and logo size, the API's JSON.

## 3. Approach
P26 f: one formatter in `InvoiceFormat` (`Amount` with the symbol; `Money` stays number-only for table cells), used by every PDF model; the symbol is not typed in a template next to a number. Only money columns get `(₹)` (not quantity, HSN, GST rate). No schema change.

## 4. Tasks (at most 8)
- [x] 1. `InvoiceFormat.Amount` (symbol, space, Indian grouping, two decimals, negative as `-₹ x`) and use it in the invoice model for subtotal, discount, taxable value, GST lines, round-off and grand total (P26 a).
- [x] 2. Invoice template: `(₹)` in the MRP, Rate, Discount and Amount headings.
- [x] 3. Credit note model and template the same way.
- [x] 4. Counter bill model and template the same way.
- [x] 5. Refund slip model and template the same way.
- [x] 6. Build at 0 warnings, 0 errors.
- [x] 7. Verify every row of section 5 on the local `book_inventory` database with the scratch API and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P26` a, b, f; `PAC25` | 1-5 | the PDF text of an invoice has `(₹)` in the money headings and `₹ ` before the grand total, the subtotal and the GST lines, and no symbol before a quantity, an HSN or a GST rate; the same for a credit note, a counter bill and a refund slip; a negative round-off reads `-₹ 0.40`; a search of the four templates finds no `₹` typed next to a value |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, PDF text and page image read back with PyMuPDF; the glyph prints as `₹`, not a box, with the fonts of this machine). Invoice: headings `MRP (₹)`, `Rate (₹)`, `Amount (₹)` and the standing amounts `₹ 500.00` (subtotal and grand total), no symbol on a quantity. Credit note: `Rate (₹)`, `Amount (₹)`, `₹ 1,000.00`, `₹ 900.00`. Counter bill (cancelled test bill): `MRP (₹)`, `Disc. (₹)`, `Amount (₹)`, `₹ 123.45` subtotal, `-₹ 0.45` round-off, `₹ 123.00` grand total. Refund slip: `Amount (₹)`, `₹ 300.00`. `InvoiceFormat.Amount` writes `₹ x` and `-₹ x`. Build 0 warnings, 0 errors. NOT seen: the GST table headings (`Taxable value (₹)`, `CGST (₹)`...) and the invoice's `Discount (₹)` column, because the local documents are GST-exempt and have no item discount; the headings are in the templates.

## 7. Risks and notes
The symbol must print as `₹` with the fonts of the production server (platform PQ4); check one real PDF there before closing, and report it as not verified if the server's PDF still cannot start (docs/Deployment_Pdf.md).
