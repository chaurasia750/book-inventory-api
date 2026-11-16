# Plan finance#14: Ledger PDF - the company's footer line

- **File:** `docs/plans/finance/2026-11-16-14-ledger-pdf-footer-line.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R26; AC18
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `finance#11` (Done, the signature block), `company#07` (Done, the invoice settings)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "ledger me footer kyo nhi aa raha")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the ledger PDF ends with the footer line the owner wrote in the invoice settings (for example "Thanks for your business"), like the invoice and the receipt. See it: set a footer line in Company profile, then download a school's ledger PDF.

## 2. Touches / leaves out
Touches: `LedgerPdfModelBuilder` (one more value from the company capability) and `Templates/ledger-body.sbn` (one line at the end). Leaves out: terms and conditions on the ledger (Backlog B141), placeholders in the footer line (platform has none for it), text and logo size, the rupee symbol.

## 3. Approach
The company's printed identity already carries `Invoice.Footer`; the builder encodes it like the other texts and the template prints it only when set. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model: `footer_line` from the company's invoice settings.
- [x] 2. Template: the centred note at the end of the last page.
- [x] 3. Build at 0 warnings, 0 errors.
- [x] 4. Verify the row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R26`, `AC18` | 1, 2 | footer line `Thanks` saved in the company: the school PDF and the vendor PDF end with `Thanks` once; after clearing it nothing is printed there |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`): footer line `Thanks for your business` saved with `PUT invoice-identity {invoice}` -> the school's ledger PDF and the vendor's ledger PDF each print it once at the end; with the footer set empty -> 0 occurrences; the footer was put back to `Thanks` afterwards. Build 0 warnings, 0 errors. NOT seen: the page image of a vendor PDF and a footer on a two-page statement's last page was read from the text only.

## 7. Risks and notes
None beyond the page end: the note must not split from the signature block (it sits inside the same kept-together block).
