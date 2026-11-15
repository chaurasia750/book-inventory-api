# Plan sales#19: Counter day summary

- **File:** `docs/plans/sales/2026-11-16-19-counter-day-summary.md`
- **Kind:** Backend
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R50; AC42
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Contract and query service for the summary (R50).
- [ ] 2. The aggregate query in the store; the endpoint and roles.
- [ ] 3. Build at 0 warnings, 0 errors; size limits.
- [ ] 4. Verify every row of section 5 on the real local database and record the result.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R50`, `AC42` | 1-2 | three live bills (Cash 100 + UPI 90, Cash 50, UPI 40) and one cancelled: 3 bills, total 280.00, Cash 150.00, UPI 130.00, the books and the quantity; the company summary adds the counters; a later bill changes the next read |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
No migration. The numbers are worked out by hand before the run.
