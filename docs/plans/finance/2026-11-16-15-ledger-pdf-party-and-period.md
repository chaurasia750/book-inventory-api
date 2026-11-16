# Plan finance#15: Ledger PDF - the party without a box and the From and To dates

- **File:** `docs/plans/finance/2026-11-16-15-ledger-pdf-party-and-period.md`
- **Kind:** Backend
- **Spec:** [../../specs/finance.md](../../specs/finance.md) - rules covered: R27; AC19
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `finance#13` (Done, the header's right side)
- **UI plan:** none, the download action is unchanged; only what the PDF prints changes
- **Approved by / on:** abhishek / 2026-11-16 (chat: "school name me border hai ... from and to use kro")
- **Created:** 2026-11-16   **Completed:** 2026-11-16

## 1. In one minute
After this plan the ledger PDF shows the school or vendor without a box, and the empty right side of that strip shows the statement's `From` and `To` dates. See it: download a school's ledger PDF with `financialYear=2026-27`.

## 2. Touches / leaves out
Touches: `Application/Finance` (a small period value worked out once from the query and the lines, the model builder and the service), `Templates/ledger-body.sbn` (the strip). Leaves out: the figures, the header, the summary, text and logo size, the rupee symbol.

## 3. Approach
One value `LedgerPeriod` (From, To as printed text) is worked out in one place from the filter and the lines, using the existing `FinancialYear.StartOf`; the model builder takes one heading object (filter words, period, made-at) instead of loose strings, so its parameter count does not grow. No schema change.

## 4. Tasks (at most 8)
- [x] 1. `LedgerPeriod` (financial year, date range, first/last line, dash) and the heading object.
- [x] 2. Model builder and service use them.
- [x] 3. Template: the borderless party strip with From and To.
- [x] 4. Build at 0 warnings, 0 errors.
- [x] 5. Verify the row of section 5 and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R27`, `AC19` | 1-3 | `financialYear=2026-27` -> `From 01 Apr 2026`, `To 31 Mar 2027`; `from=2026-11-16&to=2026-11-20` -> those two dates; no filter -> the first and last line dates; a school with no line -> dashes; the page image shows no box around the school |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, school SCH-0097 with lines and SCH-0029 without): `financialYear=2026-27` -> `From 01 Apr 2026`, `To 31 Mar 2027`; `from=2026-11-16&to=2026-11-20` -> those dates; `from=2026-11-10` -> `From 10 Nov 2026`, `To 16 Nov 2026` (the last line); no filter -> `16 Nov 2026` and `16 Nov 2026` (first and last line); a school with no line -> dashes. The page image shows no box around the school and the dates on the right. Build 0 warnings, 0 errors. NOT seen: a vendor PDF image and a session filter.

## 7. Risks and notes
The existing model builder already has more than four parameters; this plan reduces them to four by grouping, it does not add any.
