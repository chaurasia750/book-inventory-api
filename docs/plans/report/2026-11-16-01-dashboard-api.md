# Plan report#01: Dashboard API

- **File:** `docs/plans/report/2026-11-16-01-dashboard-api.md`
- **Kind:** Backend
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R1-R3, R9, R12, R13; AC1-AC9
- **Owner:** abhishek
- **Status:** Draft
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories, to be confirmed at approval)
- **Depends on:** `sales#29`, `payment#11`, `stock#06`, `purchase#05`, `price#06` (Done), `finance#03` (Done, the outstanding list)
- **UI plan:** report#02
- **Approved by / on:** pending
- **Created:** 2026-11-16

## 1. In one minute
After this plan `GET /api/v1/dashboard?session=2026-27` answers every part of the dashboard for the caller's role: payments, orders and invoices, sales, returns and refunds, what is owed, and stock.

## 2. Touches / leaves out
Touches: a new module `report`: `Domain/Report` (the dashboard session and its window), `Application/Report` (a dashboard service, one reader per part, the answer records, the message catalogue), `Api/Controllers/Report`, `docs/Code_Map.md`. Leaves out: the screen (report#02), reports as such, any table or migration.

## 3. Approach
The dashboard session value object checks the label and gives its window and start year (R3). The service builds each part with its own reader that calls the capabilities of the other modules (R1), in parallel, and wraps a failing part as `{error}` (R13); the parts the role may not see are never read (R2); the stock value pages the Main quantities from stock#06 and prices them with price R19.

## 4. Tasks (at most 8)
- [ ] 1. Domain: the dashboard session (label check, window, start year, default) (R3).
- [ ] 2. The orders, sales and returns readers over `ISalesFigures` and `IPurchaseFigures` (R7, R8).
- [ ] 3. The payments reader over `IPaymentFigures` (R4-R6).
- [ ] 4. The owed reader over the finance outstanding list (R9).
- [ ] 5. The stock reader over `IStockFigures` and `IPurchasePriceDirectory` (R10).
- [ ] 6. The dashboard service (role cut, parts in parallel, per-part errors) and the endpoint; message catalogue; the module registered (R1, R2, R13).
- [ ] 7. Build at 0 warnings, 0 errors; size limits; `docs/Code_Map.md`.
- [ ] 8. Verify AC1 to AC9 on the local database with a scratch check (money worked out first).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `AC9` | 1, 6 | `session=2026` -> `400 report.invalid_session`; no session = the current one; a future session shows zeros |
| `R2`, `AC8` | 6 | Owner sees every part; Staff have `orders`, `sales`, `stock` (no value) only; anonymous `401` |
| `R4`-`R6`, `AC1`, `AC2` | 3 | the figures of payment#11 through the endpoint |
| `R7`, `R8`, `AC3`-`AC5` | 2 | the figures of sales#29 and purchase#05 through the endpoint |
| `R9`, `AC6` | 4 | schools that owe, the five largest, advances, vendors owed |
| `R10`, `AC7` | 5 | the counts and the value (Main quantity above zero x purchase price) |
| `R13`, `AC9` | 6 | a reader that throws shows `{error}` while the others show |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Money is read, never moved, but every figure is checked against the source screens. Q1-Q4 are answered before approval.
