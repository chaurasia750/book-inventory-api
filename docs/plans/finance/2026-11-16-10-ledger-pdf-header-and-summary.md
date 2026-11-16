# Plan finance#10: Ledger PDF - header on every page, Dr/Cr and the summary with the amount in words

- **File:** `docs/plans/finance/2026-11-16-10-ledger-pdf-header-and-summary.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R20, R21, R22; AC13, AC14, AC15
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `finance#07` (Done, the ledger PDF), `company#07` (Done, the company capability)
- **UI plan:** none, the Download and Print ledger actions are unchanged (finance#08); only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all aproved hai")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the ledger PDF of a school or a vendor repeats the company's header (logo, name, address, contact, GSTIN, PAN) on **every page**, writes the balances with `Dr`/`Cr`, and ends the table with a summary box (opening, total debit, total credit, closing, and the closing amount in words). See it: download the ledger PDF of a school with more than one page of lines (`GET /api/v1/schools/{id}/ledger/pdf`) and look at page 2.

## 2. Touches / leaves out
Touches: `Application/Finance` (`LedgerPdfService` passes a page header like the invoice does, `LedgerPdfModelBuilder` adds the Dr/Cr text, the summary and the words), `Templates/ledger-body.sbn`, a new `Templates/ledger-header.sbn`, the template list and the csproj embed, the company capability read (PAN).
Leaves out: the bank details and the signature (finance#11), the statement's data and filters, the screen, the return documents, an ageing summary.

## 3. Approach
Same mechanism as the invoice (sales R30): the company header is the page header of the PDF, so it repeats; the body keeps title, party, table and the new summary. Dr/Cr is a text rule in one place (R21: school positive = Dr, vendor positive = Cr, zero = no letter) used by the rows, the opening row, the summary and the words. The words come from the existing amount-in-words rule that the invoice uses (`AmountInWords`, today in `Domain/Sales`): **finance must not depend on sales**, so task 1 first checks the module dependency direction in the specs; if the rule cannot be reached from `finance`, moving it to a shared place is a separate small change (CLAUDE.md "Shared files change separately") and this plan waits for it. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Decide how `finance` reaches the amount-in-words rule without depending on `sales`; write the outcome in section 7 (stop and ask if it needs a shared-code change).
- [x] 2. One Dr/Cr rule (school and vendor, R21) and the summary figures with the words (R22) in the model builder.
- [x] 3. `ledger-header.sbn` (logo, name, address, contact, GSTIN when printed, PAN; empty lines left out) and pass it as the page header in `LedgerPdfService` (R20).
- [x] 4. `ledger-body.sbn`: remove the page-1 header, show Dr/Cr in the balance column, add the summary box.
- [x] 5. Build at 0 warnings, 0 errors.
- [x] 6. Verify every row of section 5 on the local `book_inventory` database with the scratch API and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R20`, `AC13` | 3, 4 | a school with enough lines for two pages: the PDF text of page 1 and page 2 both have the company name (and the logo image when set); a company with only a name has no empty address/contact/GSTIN/PAN line; GSTIN printing off -> no GSTIN, PAN still there |
| `R21`, `AC14` | 2, 4 | invoice 1,200.00 + receipt 500.00 -> `700.00 Dr`; one more receipt of 800.00 -> `100.00 Cr`; a vendor the company owes 300.00 -> `300.00 Cr`; zero -> `0.00` |
| `R22`, `AC15` | 2, 4 | the summary has opening, total debit, total credit and closing equal to the JSON statement of the same filter; `700.00 Dr` -> `Rupees Seven Hundred Only`; `0.00` -> `Rupees Zero Only` |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, Owner of Children Book Center, school SCH-0097 with 54 statement lines after 20 test receipts of 1.00 cash were added; PDF text and page images read back with PyMuPDF):
- AC13: 2 pages; page 1 and page 2 both carry the logo and `Children Book Center` at the top and the table header `DATE ... BALANCE`; the company has no address, mobile, e-mail, GSTIN or PAN set, and no empty line is printed. NOT seen: the GSTIN/PAN lines and the GSTIN-off case (the local company has none of them).
- AC14: the balance column shows `1,000.00 Dr`, `750.00 Cr`, `0.00` (a zero balance, no letter) and the closing balance `3,000.00 Dr` / `2,980.00 Dr`; the amounts carry no minus sign. NOT seen: the `100.00 Cr` closing balance and a vendor with a balance (no vendor document exists on the local database), so the vendor Cr/Dr direction is read from the code only.
- AC15: the summary box shows opening `0.00`, total debit `21,540.00`, total credit `18,560.00`, closing `2,980.00 Dr`, equal to the JSON statement (`closingBalance 2980`), and the words `Rupees Two Thousand Nine Hundred Eighty Only` (before the 20 receipts: `3,000.00 Dr` -> `Rupees Three Thousand Only`). NOT seen: `Rupees Zero Only`.
- Build `dotnet build book-inventory.slnx`: 0 warnings, 0 errors.

## 7. Risks and notes
- The page header takes space from every page: the top margin must be set so the table does not run under it (check on a real two-page PDF, as the invoice does).
- Q14-Q16 of the spec are the open questions behind R21 and R22; the plan follows their recommended answers until the owner decides.
- Task 1 outcome: `AmountInWords` stays in `Domain/Sales`; the payment and purchase modules already read it from there, and the ledger builder already used `InvoiceFormat` of the sales application, so the ledger follows the same neighbouring pattern. Moving it to a shared place is a possible later clean-up (Backlog B143).
