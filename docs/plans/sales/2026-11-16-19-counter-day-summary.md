# Plan sales#19: Counter day summary

- **File:** `docs/plans/sales/2026-11-16-19-counter-day-summary.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R50; AC42
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Depends on:** `sales#17`
- **UI plan:** sales#20
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Owner and Staff can read the sales of a day for one counter or the whole company: bills, total, Cash, UPI, books and quantity. Try `GET /api/v1/counter-bills/day-summary?date=2026-11-16`.

## 2. Touches / leaves out
Touches: `Application/Sales` (a summary query service and its contract), `Infrastructure/Sales` (one aggregate query over the live bills), `Api/Controllers/Sales` (one endpoint).
Leaves out: any lock or closing of the day (Q29), the Excel and PDF reports, every screen (sales#20). No schema change.

## 3. Approach
One read-only query over live bills of the date (and the counter when given): counts and sums of the bills and their payments, the books being the line quantity of items of category Book (stored on the line as its category, R43). Cancelled bills are left out. No new table.

## 4. Tasks (at most 8)
- [x] 1. Contract and query service for the summary (R50).
- [x] 2. The aggregate query in the store; the endpoint and roles.
- [x] 3. Build at 0 warnings, 0 errors; size limits.
- [x] 4. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R50`, `AC42` | 1-2 | three live bills (Cash 100 + UPI 90, Cash 50, UPI 40) and one cancelled: 3 bills, total 280.00, Cash 150.00, UPI 130.00, the books and the quantity; the company summary adds the counters; a later bill changes the next read |

## 6. Not verified
Observed (2026-11-16, scratch API on the real `book_inventory`, fake data, Owner and Staff of Children Book Center): on a new counter with three live bills on 2026-11-16 - Book P 190 (Cash 100 + UPI 90), Copy Q x 2 = 50 (Cash 50), Book R x 2 = 40 (UPI 40) - and one cancelled bill (Book R x 1 = 20): `GET counter-bills/day-summary?date=2026-11-16&counterId=...` -> 200, `billCount 3`, `totalAmount 280.0`, `cashAmount 150.0`, `upiAmount 130.0`, `bookQuantity 3` (1 + 2), `totalQuantity 5`; a second counter with one UPI 25 bill shows 1 bill, 25.0; the company summary (no counter) is above the sum of the two (it holds the older test bills of the day: 11 bills, 1329.0); a later bill of 25 Cash makes the next read 4 bills, 305.0, Cash 175.0; a day with no bills is all zeros; no date -> 400; Staff 200; anonymous 401; company B asking with A's counter id gets 0 bills.
NOT verified: the figures for a company with a very large number of bills (no load check), and the date in the business time zone near midnight (the date is the one the caller gives).

## 7. Risks and notes
No migration. The numbers are worked out by hand before the run.
