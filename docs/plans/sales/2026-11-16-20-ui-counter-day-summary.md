# Plan sales#20: UI counter day summary

- **File:** `docs/plans/sales/2026-11-16-20-ui-counter-day-summary.md`
- **Kind:** UI
- **Spec:** [../../specs/sales.md](../../specs/sales.md) - rules covered: R50 (the screen side); AC42
- **Owner:** abhishek
- **Status:** Approved
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
- [ ] 1. Model and service call.
- [ ] 2. The summary page with the date and counter choice and its states.
- [ ] 3. Route, menu entry, i18n; build and lint; verify the row of section 5 and record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R50`, `AC42` | 2 | choose the date of the AC42 bills: the tiles show 3 bills, 280.00, Cash 150.00, UPI 130.00; a date with no bills shows the empty state |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
