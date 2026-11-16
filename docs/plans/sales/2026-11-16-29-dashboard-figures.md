# Plan sales#29: Dashboard figures of sales

- **File:** `docs/plans/sales/2026-11-16-29-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R7, R8 (the sales side); AC3, AC4, AC5
- **Owner:** abhishek
- **Status:** Draft
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories, to be confirmed at approval)
- **Depends on:** `sales#23`, `sales#25` (Done)
- **UI plan:** none, a read capability for the report module
- **Approved by / on:** pending
- **Created:** 2026-11-16

## 1. In one minute
After this plan the report module can ask sales for the figures of a window: orders and invoices of a session by status, the live invoices plus counter bills of a date range with a day-wise list, and the credit notes and refunds of a range. No endpoint.

## 2. Touches / leaves out
Touches: `Application/Sales` (a figures capability, its answer records) and `Infrastructure/Sales` (aggregate queries on orders, invoices, counter bills, credit notes, refunds), `docs/Code_Map.md`. Leaves out: the screens, payments, any change to a document.

## 3. Approach
One read-only interface `ISalesFigures` with three methods (orders of a session start year, sales of a date range with the last days by day, returns of a date range), answered by grouped queries limited to the session's company by the platform filter; a cancelled document is left out of every sum (its count is given where R7 asks for it).

## 4. Tasks (at most 8)
- [ ] 1. The capability and its answer records (R7, R8).
- [ ] 2. The orders-by-status and books-still-to-invoice queries (R7).
- [ ] 3. The sales-of-a-range and day-wise queries over invoices and counter bills (R8).
- [ ] 4. The credit notes and refunds of a range (R8).
- [ ] 5. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 6. Verify AC3 to AC5 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R7`, `AC3` | 1, 2 | the counts and totals by status against what the orders list shows; remaining quantity of confirmed orders; figures worked out first |
| `R8`, `AC4` | 3 | today's and the window's sale = live invoices + live counter bills; a cancelled one left out; the day-wise list adds up |
| `R8`, `AC5` | 4 | a credit note and a refund inside and one outside the window |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
No migration. Aggregates must not load whole tables: grouped SQL only.
