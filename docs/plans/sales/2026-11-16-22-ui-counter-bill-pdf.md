# Plan sales#22: UI counter bill Print and Download PDF

- **File:** `docs/plans/sales/2026-11-16-22-ui-counter-bill-pdf.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R51 (the screen side); AC43
- **Owner:** abhishek
- **Status:** In progress (code done and verified in headless Chrome; waiting for the owner's look in the browser)
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/sales-school-counter` for sales#15-#22, the owner's instruction "same branch pe")
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
- [x] 1. The PDF service call.
- [x] 2. Print and Download PDF buttons in the bill panel and after saving, with loading and error states.
- [x] 3. i18n; build and lint; verify the row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R51`, `AC43` | 2 | after saving a bill, Download PDF saves `CNT-2026-27-0001.pdf` with the bill's rows; Print opens the PDF in a new tab; a failure shows its message |

## 6. Not verified
The row of section 5 was checked against the running API in headless Chrome (real login, company `abc-book-center`).

Done and observed:
- Bill panel (`CNT/2026-27/0001`): Print and Download PDF render; Download PDF saved `CNT-2026-27-0001.pdf`; Print opened the receipt in a new tab (`blob:` URL).
- After saving a bill (`CNT/2026-27/0005`): Download PDF saved `CNT-2026-27-0005.pdf`; Print opened the receipt in a new tab.

**Not verified:** a failure showing its message (no failing call was forced). Build/lint as in sales#16.

## 7. Risks and notes
Seen through headless Chrome only.
