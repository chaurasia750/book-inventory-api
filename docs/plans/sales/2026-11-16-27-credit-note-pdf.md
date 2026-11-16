# Plan sales#27: Credit note A4 PDF

- **File:** `docs/plans/sales/2026-11-16-27-credit-note-pdf.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R71; AC57
- **Owner:** abhishek
- **Status:** Approved
- **Base branch:** `main` (one branch `feature/purchase-vendor-return` for price#06, sales#27, sales#28 and purchase#01-#04 in both repositories, the owner's instruction "sare plan ek hi branch pe implement krna")
- **Depends on:** `sales#23` (Done), `sales#07` (Done, the A4 invoice PDF pipeline)
- **UI plan:** sales#28
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q7, Q41, Q42 answered one by one; "sare plan ek hi branch pe implement krna")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a credit note can be downloaded as an A4 PDF titled `CREDIT NOTE`: `GET /api/v1/credit-notes/{id}/pdf`, with the lines, the condition of each, the GST summary, the total in words and the invoice it names.

## 2. Touches / leaves out
Touches: `Application/Sales` (a credit note PDF service, a model builder and an A4 template reusing the invoice header, footer and GST-summary parts), the company identity capability, `Api/Controllers/Sales` (one endpoint), the message catalogue, `docs/Code_Map.md`. Leaves out: the buttons (sales#28), the A5 refund slip, bank details and the UPI QR (Q42).

## 3. Approach
Read the credit note, fill the template through the platform template port and print it with the platform PDF renderer like `InvoicePdfService`; the rate-wise GST summary is the one rule of sales R32 (reused, never copied); a cancelled credit note prints `CANCELLED` with its reason; `Page x of y` and the repeating header as the invoice.

## 4. Tasks (at most 8)
- [ ] 1. The model builder: company header, title, credit note, school, invoice reference, reason, lines with condition, totals, GST summary, words (R71).
- [ ] 2. The A4 template reusing the invoice parts.
- [ ] 3. The PDF service and the endpoint with its roles; catalogue.
- [ ] 4. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 5. Verify AC57 on the local database: read the PDF text and look at a rendered page.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R71`, `AC57` | 1-5 | the PDF of a credit note against an invoice: A4, text has the company name, `CREDIT NOTE`, number, date, school, every line with its condition and amount, the grand total in words and the invoice number; a cancelled one shows `CANCELLED`; another company's id `404`; a GST credit note shows the rate-wise summary |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
No migration. Needs the PDF renderer to start (as sales#07). The page is looked at, not only its text.
