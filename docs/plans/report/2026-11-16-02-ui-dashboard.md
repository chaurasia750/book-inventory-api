# Plan report#02: UI dashboard (Home)

- **File:** `docs/plans/report/2026-11-16-02-ui-dashboard.md`
- **Kind:** UI
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R2, R3, R11, R12, R13 (the screen side); AC1-AC9
- **Owner:** abhishek
- **Status:** Done (2026-11-16)
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories)
- **Depends on:** report#01
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: Q1-Q4 and the dashboard choices answered one by one; "all approve")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Home page of the member app is the dashboard: the session dropdown, the quick buttons and the cards of the parts the caller may see.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): a `dashboard` page folder under `apps/member/src/app/pages/`, `libs/api-services` (a dashboard service), `libs/shared-models`, the route `/` (replacing the Welcome page), `assets/i18n/en.json`. Leaves out: the sample and vendor-order cards (R12), reports.

## 3. Approach
One card component per part with its own loading, empty and error state; the session dropdown reloads all; Owner sees every card, Staff the cards the API returns; numbers use the shared Indian-currency pipe; the day-wise sales line is a small inline SVG (no chart package); the cards link to their screens (the pending cheques to the payment list filtered to pending).

## 4. Tasks (at most 8)
- [x] 1. Shared models and the dashboard API service.
- [x] 2. The page, the session dropdown and the quick buttons (R3, R11).
- [x] 3. The payments card: received by way of payment, earlier sessions' payment, pending cheques (R4-R6).
- [x] 4. The orders and invoices card (R7).
- [x] 5. The sales card with today, the session and the day-wise line; the returns and refunds card (R8).
- [x] 6. The owed card and the stock card (R9, R10).
- [x] 7. Loading, empty and error states per card; English strings; Staff view.
- [x] 8. Build and lint clean; every row of section 5 checked in headless Chrome as Owner (and Staff if a login exists) and screenshots looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `R11` | 2 | the session dropdown reloads every card; the quick buttons open their screens ; the session dropdown (5 sessions, the current marked) reloads every card: `2025-26` shows its window 01 Apr 2025 - 31 Mar 2026 and zeros for payments and returns, today's sale unchanged; the three quick buttons (Payment for the Owner only) were seen, not clicked |
| `R4`-`R6`, `AC1`, `AC2` | 3 | the numbers equal the API's for the same session ; the payments card shows 12,800.00 received, by way of payment, 14 receipts, the earlier-session amount 2,000.00 (1 school, still due 1,500.00) and the pending cheques; equal to the API's |
| `R7`, `R8`, `AC3`-`AC5` | 4, 5 | the counts and totals equal the API's; a cancelled document is not in them ; the orders card shows Draft 2 / Confirmed 4 / Closed 54 and invoices made 6 / 13,610.00; the sales card shows today 16,736.00 and the session 19,696.00 with the line; the returns card shows credit notes 5 / 7,700.00, refunds 2 / 450.00, vendor returns 5 / 19,447.00; equal to the API's |
| `R9`, `R10`, `AC6`, `AC7` | 6 | the owed and stock cards equal the API's ; the owed card lists schools owing (2), schools in advance (9) with the five largest and vendors owed (0); the stock card shows 65 at or below zero, 8 below zero, 13 damaged and the value |
| `R2`, `R13`, `AC8`, `AC9` | 7 | Staff see no payments, owed or returns card; a failing card shows its message and a retry ; NOT run: Staff view (no staff login), a failing card with its message |

## 6. Not verified
Build and lint of `book-inventory-ui`: `npx nx build member` -> success; `npx eslint` on the dashboard folder, the model and the service -> clean.

Seen in headless Chrome as the Owner (scratch UI on 4900, API on 7900): the full dashboard for `2026-27` and for `2025-26`.

NOT verified: the Staff view, a failing part, the quick buttons clicked, the narrow (phone) width, the dark theme, loading skeletons. The Payment button opens the Schools list (a receipt is recorded in a school's profile); there is no payments screen of its own yet.

## 7. Risks and notes
Screenshots are looked at before the plan closes (UI consistency).
