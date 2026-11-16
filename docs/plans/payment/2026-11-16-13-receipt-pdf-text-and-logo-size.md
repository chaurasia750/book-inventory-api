# Plan payment#13: Larger text and logo in the receipt PDF

- **File:** `docs/plans/payment/2026-11-16-13-receipt-pdf-text-and-logo-size.md`
- **Kind:** Backend
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: P27, PAC26 (payment part)
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** none
- **UI plan:** none, only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "1 kro", with the sizes suggested in chat)
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
Larger text and logo in the receipt PDF: the text and the logo of the PDF are larger so they read well on paper. See it: download the document.

## 2. Touches / leaves out
Touches: `Templates/receipt-body.sbn` (the A5 receipt keeps its margins).
Leaves out: what the documents contain, the rupee symbol, the API, the screens.

## 3. Approach
Each template's font sizes, logo and signature heights and fixed column widths are scaled by one factor (platform P27); the page margins are raised for the taller header and footer. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Scale the sizes of the receipt template.
- [x] 2. Build at 0 warnings, 0 errors.
- [x] 3. Render the receipt and look at it.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `P27`, `PAC26` | 1-3 | the receipt renders with larger text and an 80 px logo and still fits one A5 page |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, page images read back): the text is about 1.2 times larger (body 9 px to 11 px, labels 7.5 px to 9 px, the company name 14 px to 17 px), the logo is 80 px high, the fixed column widths grew by the same factor and the page margins were raised for the taller header (A4 top margin 1.55 in, bottom 0.65 in; the ledger 1.45 in and 0.7 in). The receipt rendered (200). NOT seen: its page image and a receipt with several modes and allocations.

## 7. Risks and notes
Larger text takes more room: an A4 invoice now holds about 40 lines a page instead of 48 (sales R30 updated).
