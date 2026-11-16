# Plan report#02: UI dashboard (Home)

- **File:** `docs/plans/report/2026-11-16-02-ui-dashboard.md`
- **Kind:** UI
- **Spec:** [../../specs/report.md](../../specs/report.md) - rules covered: R2, R3, R11, R12, R13 (the screen side); AC1-AC9
- **Owner:** abhishek
- **Status:** Draft
- **Base branch:** `feature/purchase-vendor-return` (one branch `feature/report-dashboard` for sales#29, payment#11, stock#06, purchase#05, report#01 and report#02 in both repositories, to be confirmed at approval)
- **Depends on:** report#01
- **UI plan:** this is itself a UI plan
- **Approved by / on:** pending
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Home page of the member app is the dashboard: the session dropdown, the quick buttons and the cards of the parts the caller may see.

## 2. Touches / leaves out
Repository `book-inventory-ui` (build: `npx nx build member`, lint: `npx eslint` on the touched folders): a `dashboard` page folder under `apps/member/src/app/pages/`, `libs/api-services` (a dashboard service), `libs/shared-models`, the route `/` (replacing the Welcome page), `assets/i18n/en.json`. Leaves out: the sample and vendor-order cards (R12), reports.

## 3. Approach
One card component per part with its own loading, empty and error state; the session dropdown reloads all; Owner sees every card, Staff the cards the API returns; numbers use the shared Indian-currency pipe; the day-wise sales line is a small inline SVG (no chart package); the cards link to their screens (the pending cheques to the payment list filtered to pending).

## 4. Tasks (at most 8)
- [ ] 1. Shared models and the dashboard API service.
- [ ] 2. The page, the session dropdown and the quick buttons (R3, R11).
- [ ] 3. The payments card: received by way of payment, earlier sessions' payment, pending cheques (R4-R6).
- [ ] 4. The orders and invoices card (R7).
- [ ] 5. The sales card with today, the session and the day-wise line; the returns and refunds card (R8).
- [ ] 6. The owed card and the stock card (R9, R10).
- [ ] 7. Loading, empty and error states per card; English strings; Staff view.
- [ ] 8. Build and lint clean; every row of section 5 checked in headless Chrome as Owner (and Staff if a login exists) and screenshots looked at.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R3`, `R11` | 2 | the session dropdown reloads every card; the quick buttons open their screens |
| `R4`-`R6`, `AC1`, `AC2` | 3 | the numbers equal the API's for the same session |
| `R7`, `R8`, `AC3`-`AC5` | 4, 5 | the counts and totals equal the API's; a cancelled document is not in them |
| `R9`, `R10`, `AC6`, `AC7` | 6 | the owed and stock cards equal the API's |
| `R2`, `R13`, `AC8`, `AC9` | 7 | Staff see no payments, owed or returns card; a failing card shows its message and a retry |

## 6. Not verified
(filled when the plan is closed)

## 7. Risks and notes
Screenshots are looked at before the plan closes (UI consistency).
