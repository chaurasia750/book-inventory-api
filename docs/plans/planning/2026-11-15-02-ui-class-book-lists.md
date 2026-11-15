# Plan planning#02: UI class-wise book lists

- **File:** `docs/plans/planning/2026-11-15-02-ui-class-book-lists.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/planning.md](../../specs/planning.md) - rules covered: R2-R7 (the screen side); AC1, AC3, AC4
- **Owner:** abhishek
- **Status:** In progress (code done; waiting for the owner's look in the browser)
- **Depends on:** `planning#01`
- **UI plan:** this is itself a UI plan
- **Approved by / on:** abhishek / 2026-11-15 (chat: the wholesale order questions answered one by one, all with the recommended answers)
- **Created:** 2026-11-15
- **Base branch:** `main` of `book-inventory-ui` (branch `feature/sales-06`, shared by planning#02 and sales#06 on the owner's instruction)

## 1. In one minute
After this plan a school's profile has a **Book lists** tab: choose the session, see every class with its book count and student estimate, open a class to add books (search, scan, bulk tick), set quantities per student and the estimate, copy from last session or another school, and see what changed. Open a school, tab Book lists.

## 2. Touches / leaves out
Touches: `apps/member` (`pages/planning`: book lists tab, class list editor, copy dialog; the bulk item picker shared with the order editor), models, service, i18n.
Leaves out: the planning report.

## 3. Approach
Compact rows per class (U10 style inside the profile tab); the class editor reuses the scan entry and the bulk picker of sales#06; changes are saved per class.

## 4. Tasks (at most 8)
- [x] 1. Models and service.
- [x] 2. Book lists tab: session choice, class rows with counts, estimates and the difference marks.
- [x] 3. Class editor: add by scan/search/bulk, quantity per student, estimate, save, delete.
- [x] 4. Copy dialog (last session / another school, replace or not).
- [ ] 5. Build and lint; record what was seen.

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `R2`-`R7`, `AC1`, `AC3`, `AC4` | 2-4 | Create Class 5's list, copy to next session, change a book and see the marks |

## 6. Not verified
Every row of section 5 is **NOT verified** in a browser (the assistant has none): the owner opens a school's profile, tab **Book lists**, picks the session, opens a class, adds books (scan, search, "Pick many"), sets copies per student and the estimate, saves, copies from last session and from another school, and reads the marks.

Done and observed: one commit on `feature/sales-06` of `book-inventory-ui` (shared with sales#06: both change the school profile, the menu and the catalogues): `planning.model.ts`, `PlanningService`; `pages/planning`: `book-lists-tab` (session choice from `GET school-orders/session-choice`, class rows with book count, estimate and the number of changes since the previous session), `class-list-editor` (scan/search entry, the bulk picker in items-only mode, copies per student, estimate, save = `PUT`, delete), `copy-lists-dialog` (last session or another school, replace or skip); the tab joins the school profile for the Owner and Staff (Staff read-only). `npx nx run-many -t build` -> "Successfully ran target build for 3 projects"; `npx nx run member:lint` -> only the three known findings.

## 7. Risks and notes
Written without a browser.
