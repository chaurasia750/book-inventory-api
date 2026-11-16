# Plan payment#14: Receipt PDF - the school as its own paper names it, and For <company> above the signature

- **File:** `docs/plans/payment/2026-11-16-14-receipt-pdf-school-and-signatory.md`
- **Kind:** Backend
- **Spec:** [../../specs/payment.md](../../specs/payment.md) - rules covered: R21; AC16
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `payment#13` (Done, the larger receipt), `school#03` (Done, the school directory)
- **UI plan:** none, the receipt download is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "approve hai")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the receipt PDF shows the school's name, address and GSTIN (when it has one) and no internal school code, and ends with `For <company>`, the signature image and `Authorised signatory`. See it: download a receipt PDF.

## 2. Touches / leaves out
Touches: `Application/Payment` (`ReceiptPdfModelBuilder`: the school's address and GSTIN through the school directory, no code) and `Templates/receipt-body.sbn` (the school block and the signature block).
Leaves out: the school's balance on the receipt (spec Q8), the header, the modes table, the bank box, the rupee symbol, the text size.

## 3. Approach
The model builder already reads the school's name and address through the school directory; it adds the GSTIN from the same read. The template drops the code line and prints the same signature block as the ledger. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model: the school's GSTIN; stop passing the code to the template.
- [x] 2. Template: the school block (name, address, GSTIN) and `For <company>` above the signatory line.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify the rows of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R21`, `AC16` | 1-2 | a receipt of a school with an address and a GSTIN: the PDF text has the name, the address and `GSTIN ...` and no `SCH-` code; a school without a GSTIN: no GSTIN line; the page ends with `For <company name>` and `Authorised signatory`, the signature image when the company has one |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, school SCH-0097 and its receipt `RCP/2026-27/0064`; PDF text and page image read back): the school edited to an address `Test Road, Test City` and a fake GSTIN -> the receipt shows the name, the address and `GSTIN 10ABCDE1234F1Z5` under RECEIVED FROM and no `SCH-` code; the page ends with `For Children Book Center` and `Authorised signatory`; the school edited to have no GSTIN -> no GSTIN line and the address kept; the school's address and GSTIN were put back as they were. Build 0 warnings, 0 errors. NOT seen: the signature image above the line (the local company has none), a receipt with several modes and a pending cheque, and the receipt file name (unchanged, still the school code).

## 7. Risks and notes
The receipt file name keeps using the school code (R21 a says the code stays in the file name).
