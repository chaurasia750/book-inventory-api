# Plan purchase#06: Rupee symbol in the debit note PDF

- **File:** `docs/plans/purchase/2026-11-16-06-debit-note-pdf-rupee-symbol.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P26, PAC25 (purchase part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#30` (the shared amount formatter)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "jo plan hai wo approved hai impmemnent kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the vendor debit note PDF writes `(₹)` in its money headings and `₹ 1,200.00` on every total. See it: download a vendor return's debit note.

## 2. Touches / leaves out
Touches: `Application/Purchase` (`DebitNotePdfModelBuilder`) and `Templates/debit-note-body.sbn`.
Leaves out: the other PDFs, the screens, text and logo size.

## 3. Approach
Same rule and formatter as sales#30. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model: totals and GST lines through the shared amount formatter.
- [x] 2. Template: `(₹)` in the money headings.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P26` a, b; `PAC25` | 1, 2 | a debit note PDF: money headings end with `(₹)`, the grand total reads `₹ ...`, quantities and GST rates have no symbol |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, PDF text and page image read back with PyMuPDF; the glyph prints as `₹`, not a box, with the fonts of this machine). Debit note PDF: `Rate (₹)`, `Amount (₹)`, `₹ 360.00` subtotal and total; quantities have no symbol.

## 7. Risks and notes
The symbol must print as `₹` with the fonts of the production server (platform PQ4); check one real PDF there before closing, and report it as not verified if the server's PDF still cannot start (docs/Deployment_Pdf.md).
