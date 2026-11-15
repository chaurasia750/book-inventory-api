# Plan sales#22: UI counter bill Print and Download PDF

- **File:** `docs/plans/sales/2026-11-16-22-ui-counter-bill-pdf.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R51 (the screen side); AC43
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** sales#21
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a counter bill (right after saving and in its detail) has **Print** and **Download PDF** buttons for the A5 receipt.

## 2. Touches / leaves out
Touches: `libs/api-services` (the PDF call), `apps/member` (`pages/counters`: the bill panel and the page shown after saving), `assets/i18n/en.json`.
Leaves out: thermal printing.

## 3. Approach
The buttons work like the payment receipt's (the tab for the print dialog is opened first so a pop-up blocker allows it; the download is named after the bill number); a spinner while the PDF is made; an error shows its catalogue text.

## 4. Tasks (at most 8)
- [ ] 1. The PDF service call.
- [ ] 2. Print and Download PDF buttons in the bill panel and after saving, with loading and error states.
- [ ] 3. i18n; build and lint; verify the row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R51`, `AC43` | 2 | after saving a bill, Download PDF saves `CNT-2026-27-0001.pdf` with the bill's rows; Print opens the PDF in a new tab; a failure shows its message |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Seen through headless Chrome only.
