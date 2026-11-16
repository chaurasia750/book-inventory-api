# Plan sales#31: Larger text and logo in the invoice, credit note, counter bill and refund slip PDFs

- **File:** `docs/plans/sales/2026-11-16-31-pdf-text-and-logo-size.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P27, PAC26 (sales part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** none
- **UI plan:** none, only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "1 kro", with the sizes suggested in chat)
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
Larger text and logo in the invoice, credit note, counter bill and refund slip PDFs: the text and the logo of the PDF are larger so they read well on paper. See it: download the document.

## 2. Touches / leaves out
Touches: the templates `invoice-*.sbn`, `credit-note-*.sbn`, `counter-bill-body.sbn`, `refund-slip-body.sbn` and the page margins of `InvoicePdfService` and `CreditNotePdfService`.
Leaves out: what the documents contain, the rupee symbol, the API, the screens.

## 3. Approach
Each template's font sizes, logo and signature heights and fixed column widths are scaled by one factor (platform P27); the page margins are raised for the taller header and footer. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Scale the sizes of the four documents' templates.
- [x] 2. Raise the margins of the A4 documents.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Render each PDF and look at it.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P27`, `PAC26` | 1-4 | the invoice, credit note, counter bill and refund slip render with larger text and an 80 px logo; no amount wraps; the header sits above the table |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, page images read back): the text is about 1.2 times larger (body 9 px to 11 px, labels 7.5 px to 9 px, the company name 14 px to 17 px), the logo is 80 px high, the fixed column widths grew by the same factor and the page margins were raised for the taller header (A4 top margin 1.55 in, bottom 0.65 in; the ledger 1.45 in and 0.7 in). Invoice, credit note, counter bill and refund slip were rendered; the invoice page image was read. NOT seen: a long invoice of several pages with the larger text (the header on page 2, the page break between lines) and the credit note, counter bill and refund slip page images (only their text was read).

## 7. Risks and notes
Larger text takes more room: an A4 invoice now holds about 40 lines a page instead of 48 (sales R30 updated).
