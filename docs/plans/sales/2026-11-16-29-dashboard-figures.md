# Plan sales#29: Dashboard figures of sales

- **File:** `docs/plans/sales/2026-11-16-29-dashboard-figures.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R7, R8 (the sales side); AC3, AC4, AC5
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories)
- **Depends on:** `sales#23`, `sales#25` (Done)
- **UI plan:** none, a read capability for the report module
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q4 and the dashboard choices answered one by one; "all approve")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the report module can ask sales for the figures of a window: orders and invoices of a session by status, the live invoices plus counter bills of a date range with a day-wise list, and the credit notes and refunds of a range. No endpoint.

## 2. Touches / leaves out
Touches: `Application/Sales` (a figures capability, its answer records) and `Infrastructure/Sales` (aggregate queries on orders, invoices, counter bills, credit notes, refunds), `docs/Code_Map.md`. Leaves out: the screens, payments, any change to a document.

## 3. Approach
One read-only interface `ISalesFigures` with three methods (orders of a session start year, sales of a date range with the last days by day, returns of a date range), answered by grouped queries limited to the session's company by the platform filter; a cancelled document is left out of every sum (its count is given where R7 asks for it).

## 4. Tasks (at most 8)
- [x] 1. The capability and its answer records (R7, R8).
- [x] 2. The orders-by-status and books-still-to-invoice queries (R7).
- [x] 3. The sales-of-a-range and day-wise queries over invoices and counter bills (R8).
- [x] 4. The credit notes and refunds of a range (R8).
- [x] 5. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [x] 6. Verify AC3 to AC5 on the local database with a scratch check.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R7`, `AC3` | 1, 2 | the counts and totals by status against what the orders list shows; remaining quantity of confirmed orders; figures worked out first ; order of 10 x 250: Draft +1 / +2500, after confirm Confirmed +1 and 10 books still to invoice, after the invoice Closed +1 and live invoices +1 / +2500; an order of session `2025-26` is not in the `2026-27` orders and is in the `2025-26` ones |
| `R8`, `AC4` | 3 | today's and the window's sale = live invoices + live counter bills; a cancelled one left out; the day-wise list adds up ; today's sale +2500 (invoices +1) and the session's sale +2500 after the invoice; an invoice of 1000 dated today in an earlier session is in the sale (+1000); the last day of the line is today and carries today's total |
| `R8`, `AC5` | 4 | a credit note and a refund inside and one outside the window ; a credit note of 2 books (500) from the invoice: credit notes +1 / +500 |

## 6. Not verified
Build: `dotnet build src/BookInventory.Api` to a scratch output (the owner's own API holds the default `bin`) -> 0 warnings, 0 errors.

Scratch check on the real `book_inventory` (scratch API on 7900, owner login): 31 checks, all passed; the dashboard was read before and after each step and the deltas compared with figures worked out first.

Added while building (R6): `EarlierSessionInvoicesAsync(startYear)`, the live invoices whose session starts before the year (the payments reader needs them).

NOT verified: a refund inside and one outside the window (only the credit note part of AC5), a cancelled counter bill left out, the day-wise list on a second day.

## 7. Risks and notes
No migration. Aggregates must not load whole tables: grouped SQL only.
