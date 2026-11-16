# Plan payment#12: Rupee symbol in the payment receipt PDF

- **File:** `docs/plans/payment/2026-11-16-12-receipt-pdf-rupee-symbol.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P26, PAC25 (payment part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#30` (the shared amount formatter)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "jo plan hai wo approved hai impmemnent kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the payment receipt PDF writes `(₹)` in its money column headings and `₹ 1,200.00` on the total received and every standing amount. See it: download a receipt PDF.

## 2. Touches / leaves out
Touches: `Application/Payment` (`ReceiptPdfModelBuilder` uses the shared formatter) and `Templates/receipt-body.sbn` (the typed `₹ ` next to the total goes; headings `Amount (₹)`).
Leaves out: the other PDFs, the screens, text and logo size.

## 3. Approach
Same rule and formatter as sales#30; the receipt already types `₹ ` once in its template, which moves into the model so the symbol has one source (P26 f). No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model: the total received, each applied amount and the advance through the shared amount formatter where they stand alone.
- [x] 2. Template: remove the typed `₹ `, add `(₹)` to the Amount headings of the Mode table and the Applied-to-invoice table.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P26` a, b, f; `PAC25` | 1, 2 | a receipt with two modes and one allocation: the PDF has `Amount (₹)` in both tables and `₹ 1,000.00` on the total received; an advance reads `₹ 100.00`; the receipt template has no typed `₹` |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, PDF text and page image read back with PyMuPDF; the glyph prints as `₹`, not a box, with the fonts of this machine). Receipt PDF: `Amount (₹)` in the mode table and `₹ 1.00` on the total received; the typed `₹ ` is gone from the template. NOT seen: a receipt with an allocation and an advance (their `Amount (₹)` heading and cells are in the template).

## 7. Risks and notes
The symbol must print as `₹` with the fonts of the production server (platform PQ4); check one real PDF there before closing, and report it as not verified if the server's PDF still cannot start (docs/Deployment_Pdf.md).
