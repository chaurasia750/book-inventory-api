# Plan finance#13: Ledger PDF - title and period in the right side of the header

- **File:** `docs/plans/finance/2026-11-16-13-ledger-pdf-header-right-side.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R20 (the 2026-11-16 sentence); AC13
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `finance#10` (Done, the header on every page)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: the blank header side was shown and "recommended" answered)
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the right side of the ledger PDF's header, which is blank today, shows `LEDGER STATEMENT` and the period on every page, and the body no longer repeats them. See it: download a school's ledger PDF.

## 2. Touches / leaves out
Touches: `Templates/ledger-header.sbn` (the right block) and `Templates/ledger-body.sbn` (the title line and the party box's period go). Leaves out: the figures, the summary, the bank box, text and logo size, the rupee symbol (platform P26).

## 3. Approach
The header template already receives the whole model, so it reads the existing `filter`; nothing new is computed. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Header: the right block with the title and the period.
- [x] 2. Body: remove the centred title and the period of the party box.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify the row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC13` | 1, 2 | a two-page PDF: both pages show `LEDGER STATEMENT` and the period in the header's right side; the body has the title and the period once each in total (not twice) |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, school SCH-0097, 2 pages; text and page image read back): page 1 and page 2 each have `LEDGER STATEMENT` once and `Period` once, in the header's right side next to the company block; the body starts with the party box; the header stays inside the top margin; build 0 warnings, 0 errors. NOT seen: a vendor PDF image and a company with a long address (the right block is `nowrap`, a very long title is not expected).

## 7. Risks and notes
The header must stay inside the top margin (1.0 in): check on a real two-page PDF.
