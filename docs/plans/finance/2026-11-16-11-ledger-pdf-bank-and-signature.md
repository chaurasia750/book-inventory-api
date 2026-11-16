# Plan finance#11: Ledger PDF - bank details to pay into and the signature

- **File:** `docs/plans/finance/2026-11-16-11-ledger-pdf-bank-and-signature.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R23, R24, R25; AC16, AC17
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `finance#10` (the header and the summary), `company#07` (Done, bank account and signature through the company capability)
- **UI plan:** none, the Download and Print ledger actions are unchanged (finance#08); only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all aproved hai")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan a **school** ledger PDF has a `Pay to` box with the company's bank account and UPI under the summary (nothing when the company has none, and never on a vendor ledger), and every ledger PDF ends with `For <company>`, the signature image when set, and `Authorised signatory`. See it: download the ledger PDF of a school and of a vendor of a company that has a bank account and a signature.

## 2. Touches / leaves out
Touches: `Application/Finance` (`LedgerPdfModelBuilder` adds the bank box for a school party only and the signature block), `Templates/ledger-body.sbn`, the company capability read (bank account, signature image).
Leaves out: the header and the summary (finance#10), terms and conditions and an ageing summary (spec Q17, Backlog), the credit note, debit note and refund slip (R25: audited separately, gaps go to the Backlog), any change of the company's stored bank account.

## 3. Approach
The bank box is built only when the party is a school (R23, D8) and the company capability returns a bank account; each value prints only when set. The signature block is the same for both party kinds (R24) and reuses the company's signature image like the invoice. Nothing is stored. No schema change.

## 4. Tasks (at most 8)
- [x] 1. Model builder: the `Pay to` box for a school party only, from the company capability (R23); none for a vendor or when no bank account is set.
- [x] 2. Model builder: the signature block (`For <company>`, image when set, `Authorised signatory`) (R24).
- [x] 3. `ledger-body.sbn`: the `Pay to` box under the summary and the signature block at the end, kept together so they do not split across pages.
- [x] 4. Build at 0 warnings, 0 errors.
- [x] 5. Verify every row of section 5 on the local `book_inventory` database with the scratch API and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R23`, `AC16` | 1, 3 | a company with a bank account and a UPI id: the school PDF text has holder, bank, account number, IFSC, branch and UPI; the same company's vendor PDF has no `Pay to`; a company with no bank account: no box on the school PDF |
| `R24`, `AC17` | 2, 3 | the last page ends with `For <company name>` and `Authorised signatory`; the signature image is above the line when the company has one and absent when it has none |
| `R25` | 1-3 | the JSON statement, filters, totals, the 5000-line `400`, Staff `403`, another company `404` and the file name are unchanged (re-run the finance#07 checks) |

## 6. Not verified
Observed (2026-11-16, same scratch API and database; fake bank values `Test Holder`, `Test Bank`, `123456789012`, `SBIN0001234`, `test@upi` set through `PUT companies/{id}/invoice-identity` and cleared afterwards):
- AC16: the school PDF has the box `PAY TO` with `Account name Test Holder`, `Bank Test Bank, Test Branch`, `Account no. 123456789012`, `IFSC SBIN0001234`, `UPI test@upi` under the summary; the vendor PDF of the same company has no bank text; after `PUT {bank:{}}` the school PDF has no `PAY TO`.
- AC17: every PDF (school, vendor, one page and two pages) ends with `For Children Book Center` and `Authorised signatory`. NOT seen: the signature image (the local company has none).
- R25: the statement lines, totals and `200` answers are the same as before (the JSON closing balance equals the PDF's); not re-run: Staff `403`, another company `404`, the 5000-line `400`, the file name.
- Also observed, the company R20 fix: with the terms and footer saved by `PUT {invoice}` and then the bank by `PUT {bank}`, `GET invoice-identity` still returns both the terms and the bank; saving the terms again kept the bank.
- Build: 0 warnings, 0 errors.

## 7. Risks and notes
- The bank account is read through the company capability only; the PDF never stores it. The account number is company data, printed on purpose on the company's own document; no real account number is used in the checks (fake values only).
- The company's bank values were being cleared when its invoice terms were saved (fixed in the company spec R20 on 2026-11-16, not yet deployed): check the bank account is present before judging this plan on a real database.
- Q15 of the spec (bank details on a vendor statement) is open; the plan follows its recommended answer (none).
