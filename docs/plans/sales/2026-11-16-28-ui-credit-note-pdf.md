# Plan sales#28: UI credit note Print and Download PDF

- **File:** `docs/plans/sales/2026-11-16-28-ui-credit-note-pdf.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R71 (the screen side); AC57
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** sales#27
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the credit note panel beside the Returns list has **Print** and **Download PDF** buttons for the A4 credit note.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): the credit note panel in `apps/member/src/app/pages/returns/`, `libs/api-services` (one method `creditNotePdf`), `assets/i18n/en.json`. Leaves out: any new screen.

## 3. Approach
The two buttons work like those of the refund slip and the counter bill: the PDF is fetched as a blob, opened in a new tab for Print and saved as `<number>.pdf` for Download; a failure shows the API message.

## 4. Tasks (at most 8)
- [ ] 1. The API service method for the PDF.
- [ ] 2. Print and Download PDF in the credit note panel with loading and error states; English strings.
- [ ] 3. Build and lint clean; the buttons clicked in headless Chrome and the downloaded file looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R71`, `AC57` | 1-3 | Download PDF saves `CN-2026-27-....pdf`; Print opens the PDF in a new tab; a forced failure shows its message |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Seen through headless Chrome only; screenshots are looked at before the plan closes.
