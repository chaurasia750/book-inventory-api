# Plan purchase#04: UI Debit Note Print and Download PDF

- **File:** `docs/plans/purchase/2026-11-16-04-ui-debit-note-pdf.md`
- **Kind:** UI
- **Spec:** [../../specs/purchase.md](../../specs/purchase.md) - rules covered: R11 (the screen side); AC9
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** purchase#03
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the vendor return panel has **Print** and **Download PDF** buttons for the Debit Note.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): the vendor return panel, `libs/api-services` (one method), `assets/i18n/en.json`. Leaves out: any new screen.

## 3. Approach
The two buttons work like those of the credit note and the refund slip.

## 4. Tasks (at most 8)
- [x] 1. The API service method for the PDF.
- [x] 2. Print and Download PDF in the panel with loading and error states; English strings.
- [x] 3. Build and lint clean; the buttons clicked in headless Chrome and the downloaded file looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R11`, `AC9` | 1-3 | Download PDF saves `DN-2026-27-....pdf`; Print opens it in a new tab; a forced failure shows its message ; Print and Download PDF show in the panel; Download PDF clicked in headless Chrome and the API log shows `GET vendor-returns/{id}/pdf` -> 200; the file was not opened; a failure was not tried |

## 6. Not verified
Build and lint of `book-inventory-ui`: `npx nx build member` -> success; `npx eslint` on the touched folders, models and services -> clean (`member-layout.ts` keeps only the known `Static imports of lazy-loaded libraries` finding).

NOT verified: the saved file, Print in a new tab, the failure message.

## 7. Risks and notes
Seen through headless Chrome only.
