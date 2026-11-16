# Plan finance#12: Rupee symbol in the ledger statement PDF

- **File:** `docs/plans/finance/2026-11-16-12-ledger-pdf-rupee-symbol.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P26, PAC25 (finance part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#30` (the shared amount formatter), `finance#11` (Done, the summary and the bank box)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "jo plan hai wo approved hai impmemnent kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the ledger PDF writes `(₹)` in the Debit, Credit and Balance headings and `₹ 1,200.00 Dr` on the summary figures. See it: download a school's ledger PDF.

## 2. Touches / leaves out
Touches: `Application/Finance` (`LedgerPdfModelBuilder`: the summary's figures stand alone and get the symbol, a row's balance does not) and `Templates/ledger-body.sbn` (headings).
Leaves out: the other PDFs, the screens, text and logo size, the Dr/Cr rule itself (finance R21).

## 3. Approach
Same formatter as sales#30; in the table column the balance stays `1,200.00 Dr` (number and letter), standing alone it is `₹ 1,200.00 Dr`. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model: the summary's opening, total debit, total credit and closing through the shared amount formatter, the Dr/Cr letter kept.
- [x] 2. Template: `(₹)` in the Debit, Credit and Balance headings.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify every row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P26` a, b; `PAC25` | 1, 2 | a school PDF: headings `Debit (₹)`, `Credit (₹)`, `Balance (₹)`; the summary reads `₹ 2,980.00 Dr` for the closing balance and `₹ 21,540.00` for the total debit; the table cells have no symbol; the words line has none |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, PDF text and page image read back with PyMuPDF; the glyph prints as `₹`, not a box, with the fonts of this machine). School ledger PDF: headings `DEBIT (₹)`, `CREDIT (₹)`, `BALANCE (₹)`; the summary reads `₹ 0.00`, `₹ 21,540.00`, `₹ 18,560.00` and `₹ 2,980.00 Dr`; the table cells carry no symbol and keep `Dr`/`Cr`.

## 7. Risks and notes
The symbol must print as `₹` with the fonts of the production server (platform PQ4); check one real PDF there before closing, and report it as not verified if the server's PDF still cannot start (docs/Deployment_Pdf.md).
