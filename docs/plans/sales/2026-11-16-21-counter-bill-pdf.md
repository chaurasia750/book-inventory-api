# Plan sales#21: Counter bill receipt PDF

- **File:** `docs/plans/sales/2026-11-16-21-counter-bill-pdf.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R51; AC43
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `sales#17`, `sales#07` (Done, the PDF pipeline)
- **UI plan:** sales#22
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan a counter bill can be downloaded as an A5 PDF receipt: `GET /api/v1/counter-bills/{id}/pdf`.

## 2. Touches / leaves out
Touches: `Application/Sales` (a receipt PDF service, model builder and one embedded A5 template, like the payment receipt), the company identity capability, `Api/Controllers/Sales` (one endpoint), the message catalogue, the module template folder in the csproj. No schema change.
Leaves out: the Print and Download buttons (sales#22), the thermal roll format.

## 3. Approach
The service reads the bill (a cancelled one is marked `CANCELLED`), fills one template through the platform template port and renders it with the platform PDF renderer, like `ReceiptPdfService`; the title and the rate-wise GST summary follow R51 and R32 through the same helpers as the invoice (amount in words, Indian grouping).

## 4. Tasks (at most 8)
- [ ] 1. The model builder: company header, title, bill, student, lines, totals, GST summary, payments, words (R51).
- [ ] 2. The A5 template.
- [ ] 3. The PDF service and the endpoint with its roles; catalogue.
- [ ] 4. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 5. Verify AC43 on the local database: read the PDF text and a rendered page.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R51`, `AC43` | 1-4 | the PDF of a GST-off bill: A5, text has the company name, `CASH MEMO`, `GST Exempted`, the number, date, student and class, every line, the grand total in words, Cash and UPI amounts; a GST bill says `TAX INVOICE` with the GST summary; a cancelled bill says `CANCELLED`; another company 404 |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration. Needs the PDF renderer to start (as sales#07).
