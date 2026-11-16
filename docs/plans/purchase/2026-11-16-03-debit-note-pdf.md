# Plan purchase#03: Debit Note A4 PDF

- **File:** `docs/plans/purchase/2026-11-16-03-debit-note-pdf.md`
- **Kind:** Backend
- **Spec:** [../../specs/purchase.md](../../specs/purchase.md) - rules covered: R11; AC9
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** `purchase#01`, `sales#27` (the credit note PDF it is made like)
- **UI plan:** purchase#04
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a vendor return can be downloaded as an A4 PDF titled `DEBIT NOTE`: `GET /api/v1/vendor-returns/{id}/pdf`.

## 2. Touches / leaves out
Touches: `Application/Purchase` (a Debit Note PDF service, model builder and A4 template), the company identity capability, `Api/Controllers/Purchase` (one endpoint), the message catalogue, `docs/Code_Map.md`. Leaves out: the buttons (purchase#04).

## 3. Approach
Read the vendor return, fill the template through the platform template port and print it with the platform PDF renderer, like the credit note PDF; a cancelled return prints `CANCELLED`; `Page x of y` and the repeating header.

## 4. Tasks (at most 8)
- [ ] 1. The model builder: company header, title, number, date, vendor, lines with stock source, total, round-off, words (R11).
- [ ] 2. The A4 template.
- [ ] 3. The PDF service and the endpoint with its roles; catalogue.
- [ ] 4. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 5. Verify AC9 on the local database: read the PDF text and look at a rendered page.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R11`, `AC9` | 1-5 | the PDF: A4, text has the company name, `DEBIT NOTE`, number, date, vendor, every line, the total in words; a cancelled one shows `CANCELLED`; another company's id `404`; the page looked at |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
No migration. Needs the PDF renderer to start.
