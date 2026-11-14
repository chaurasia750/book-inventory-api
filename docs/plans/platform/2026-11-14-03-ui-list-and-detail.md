# Plan platform#03: UI list and detail (record opens beside the list)

- **File:** `docs/plans/platform/2026-11-14-03-ui-list-and-detail.md`
- **Kind:** UI (repository `book-inventory-ui`; build `npx nx run-many -t build`, lint `npx nx run-many -t lint`)
- **Spec:** [../../specs/platform.md](../../specs/platform.md) - rules covered: U10; UAC6
- **Owner:** abhishek
- **Status:** In progress
- **Depends on:** platform#02
- **UI plan:** this is itself a UI plan, no backend part
- **Approved by / on:** abhishek / 2026-11-14 (chat: wants record details to open like the Purchase Order details of `tulas-international-school-ui`; scope Schools, Staff, Companies, Users; ownership confirmed)
- **Created:** 2026-11-14
- **Base branch:** `feature/school-screens-ui` (UI repo, owner's standing instruction: work on the current branch)

## 1. In one minute
After this plan, clicking a row in Schools, Staff, Companies or Users no longer jumps to a separate page: the list narrows to the left and the record opens on the right with an X to close it. Open any of the four lists and click a row.

## 2. Touches / leaves out
**Taken from `tulas-international-school-ui` (owner confirmed ownership, 2026-11-14):** the list-and-detail layout, the Close button and the "selected id from the child route" helper. Adapted to this project's palette.
Touches: `libs/ui` (new `ListDetailComponent`, `CloseButtonComponent`, `injectSelectedId`), the list and detail components of Schools and Staff (`member`) and Companies and Users (`admin`), their route definitions (a child route `:id` per list; old `<list>/:id` URLs keep working).
Leaves out: the reference's filter bar and document history, any API change, new fields on the records.

## 3. Approach
The list page owns the layout and reads the open id from the child route; the detail component takes the id as an input, reloads when it changes, and emits `closed`. Server calls and texts of each detail stay as they are.

## 4. Tasks (at most 8)
- [x] 1. Shared: `ListDetailComponent`, `CloseButtonComponent`, `injectSelectedId` in `libs/ui` (U10).
- [x] 2. Schools: list with compact rows, detail as panel, route child `:id` (U10).
- [x] 3. Staff: same (U10).
- [x] 4. Companies: same (U10).
- [x] 5. Users: same (U10).
- [x] 6. `npx nx run-many -t build` and `-t lint`: 0 new findings against the base.
- [ ] 7. Look at the four lists in a browser and record what was seen (UAC6).

## 5. Spec coverage
| Rule / AC | Task | Verification (input -> observed output) |
|---|---|---|
| `U10` | 1-5 | Build passes; code reviewed against the reference |
| `UAC6` | 7 | Click a row, X, reload on `/<list>/:id`, phone width, on each of the four lists |

## 6. Not verified
To be filled when the plan is closed (no browser tool for the assistant: the owner looks at the screens).

## 7. Risks and notes
A detail component keeps its own load and actions; only how it is placed changes. Users detail is the largest (442 lines) and is converted last.
