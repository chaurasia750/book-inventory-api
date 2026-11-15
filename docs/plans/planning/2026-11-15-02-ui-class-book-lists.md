# Plan planning#02: UI class-wise book lists

- **File:** `docs/plans/planning/2026-11-15-02-ui-class-book-lists.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/planning.md](../../specs/planning.md) - rules covered: R2-R7 (the screen side); AC1, AC3, AC4
- **Owner:** abhishek
- **Status:** Approved
- **Depends on:** `planning#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15

## 1. In one minute
After this plan a school's profile has a **Book lists** tab: choose the session, see every class with its book count and student estimate, open a class to add books (search, scan, bulk tick), set quantities per student and the estimate, copy from last session or another school, and see what changed. Open a school, tab Book lists.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/planning`: book lists tab, class list editor, copy dialog; the bulk item picker shared with the order editor), models, service, i18n.
Leaves out: the planning report.

## 3. Approach
Compact rows per class (U10 style inside the profile tab); the class editor reuses the scan entry and the bulk picker of sales#06; changes are saved per class.

## 4. Tasks (at most 8)
- [ ] 1. Models and service.
- [ ] 2. Book lists tab: session choice, class rows with counts, estimates and the difference marks.
- [ ] 3. Class editor: add by scan/search/bulk, quantity per student, estimate, save, delete.
- [ ] 4. Copy dialog (last session / another school, replace or not).
- [ ] 5. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`-`R7`, `AC1`, `AC3`, `AC4` | 2-4 | Create Class 5's list, copy to next session, change a book and see the marks |

## 6. Not verified
To be filled when the plan is closed.

## 7. Risks and notes
Written without a browser.
