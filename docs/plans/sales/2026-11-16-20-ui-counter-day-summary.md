# Plan sales#20: UI counter day summary

- **File:** `docs/plans/sales/2026-11-16-20-ui-counter-day-summary.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R50 (the screen side); AC42
- **Owner:** abhishek
- **Status:** In progress (code done and verified in headless Chrome; waiting for the owner's look in the browser)
- **Base branch:** `main` of `book-inventory-ui` (one branch `feature/sales-school-counter` for sales#15-#22, the owner's instruction "same branch pe")
- **Depends on:** sales#19
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-16 (chat: "all plan approve hai, ek hi branch pe")
- **Created:** 2026-11-16

## 1. In one minute
After this plan the Counters menu has a **Day summary** page: pick a date (and optionally a counter) and see the bills, total, Cash, UPI, books and quantity. Open `/<company>/counters/day-summary`.

## 2. Touches / leaves out
Touches: `libs/shared-models`, `libs/api-services`, `apps/member` (`pages/counters`: the summary page and its route), `assets/i18n/en.json`.
Leaves out: printing or exporting the summary.

## 3. Approach
A date picker (today by default, business time zone) and a counter select; the figures as large tiles with the totals in Indian grouping; loading, empty (no bills that day), error with retry and success states.

## 4. Tasks (at most 8)
- [x] 1. Model and service call.
- [x] 2. The summary page with the date and counter choice and its states.
- [x] 3. Route, menu entry, i18n; build and lint; verify the row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R50`, `AC42` | 2 | choose the date of the AC42 bills: the tiles show 3 bills, 280.00, Cash 150.00, UPI 130.00; a date with no bills shows the empty state |

## 6. Not verified
The row of section 5 was checked against the running API in headless Chrome (real login, company `abc-book-center`).

Done and observed: the Day summary for 2026-11-16 showed Bills 3, Total 280.00, Cash 150.00, UPI 130.00, Books 3, Quantity 5; 2026-11-01 showed "No bills on this day.".

**Not verified:** the counter select was not exercised (only the all-counters default). Build/lint as in sales#16.

## 7. Risks and notes
Written without a browser.
